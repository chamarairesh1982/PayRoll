using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.StatutoryReports;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Services;

public class StatutoryReportService : IStatutoryReportService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStatutoryReportStorage _storage;
    private readonly EpfEtfReportExporterResolver _exporterResolver = new();

    public StatutoryReportService(
        IPayrollDbContext dbContext,
        ICurrentUserService currentUserService,
        IStatutoryReportStorage storage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _storage = storage;
    }

    public async Task<EpfEtfReportResultDto> GenerateEpfEtfReportAsync(
        EpfEtfReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .FirstOrDefaultAsync(pr => pr.Id == request.PayRunId, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found.");
        }

        if (payRun.Status != PayRunStatus.Locked && !payRun.IsLocked)
        {
            throw new InvalidOperationException("EPF/ETF reports can only be generated for locked pay runs.");
        }

        var rows = new List<EpfEtfReportRow>();
        var employeeDtos = new List<EpfEtfReportEmployeeDto>();
        var warnings = new List<EpfEtfReportWarningDto>();

        foreach (var paySlip in payRun.PaySlips)
        {
            var employee = paySlip.Employee;
            var baseAmount = RoundCurrency(paySlip.Earnings.Where(e => e.IsEpfApplicable || e.IsEtfApplicable).Sum(e => e.Amount));

            rows.Add(new EpfEtfReportRow(
                employee?.EmployeeCode,
                employee?.FullName,
                employee?.NicNumber,
                employee?.EpfNumber,
                baseAmount,
                paySlip.EmployeeEpf,
                paySlip.EmployerEpf,
                paySlip.EmployerEtf));

            employeeDtos.Add(new EpfEtfReportEmployeeDto
            {
                PaySlipId = paySlip.Id,
                EmployeeId = paySlip.EmployeeId,
                EmployeeCode = employee?.EmployeeCode,
                EmployeeName = employee?.FullName,
                NicNumber = employee?.NicNumber,
                EpfNumber = employee?.EpfNumber,
                ContributableBase = baseAmount,
                EmployeeEpf = paySlip.EmployeeEpf,
                EmployerEpf = paySlip.EmployerEpf,
                EmployerEtf = paySlip.EmployerEtf
            });

            if (employee is not null)
            {
                if (string.IsNullOrWhiteSpace(employee.NicNumber))
                {
                    warnings.Add(new EpfEtfReportWarningDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.FullName,
                        Message = "Missing NIC number."
                    });
                }

                if (string.IsNullOrWhiteSpace(employee.EpfNumber))
                {
                    warnings.Add(new EpfEtfReportWarningDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.FullName,
                        Message = "Missing EPF number."
                    });
                }
            }
            else
            {
                warnings.Add(new EpfEtfReportWarningDto
                {
                    EmployeeId = paySlip.EmployeeId,
                    EmployeeCode = null,
                    EmployeeName = null,
                    Message = "Employee record missing for payslip."
                });
            }
        }

        var summary = new EpfEtfReportSummary(
            employeeDtos.Count,
            RoundCurrency(employeeDtos.Sum(r => r.ContributableBase)),
            RoundCurrency(employeeDtos.Sum(r => r.EmployeeEpf)),
            RoundCurrency(employeeDtos.Sum(r => r.EmployerEpf)),
            RoundCurrency(employeeDtos.Sum(r => r.EmployerEtf)));

        var reportData = new EpfEtfReportData(
            payRun.Code,
            payRun.Name,
            payRun.PeriodStart,
            payRun.PeriodEnd,
            payRun.PayDate,
            summary,
            rows);

        var exportFormat = string.IsNullOrWhiteSpace(request.Format) ? "csv" : request.Format.Trim();
        var exporter = _exporterResolver.Resolve(exportFormat);
        var content = exporter.Render(reportData);
        var bytes = Encoding.UTF8.GetBytes(content);

        var generatedAt = DateTime.UtcNow;
        var fileName = $"{payRun.Code}-EPFETF-{generatedAt:yyyyMMddHHmmss}.{exporter.FileExtension}";
        var filePath = await _storage.SaveAsync(fileName, bytes, cancellationToken);

        FileExportResultDto? warningFile = null;
        string? warningFilePath = null;
        string? warningFileName = null;
        string? warningContentType = null;

        if (warnings.Count > 0)
        {
            var warningsContent = BuildWarningCsv(warnings);
            var warningBytes = Encoding.UTF8.GetBytes(warningsContent);
            warningFileName = $"{payRun.Code}-EPFETF-Warnings-{generatedAt:yyyyMMddHHmmss}.csv";
            warningFilePath = await _storage.SaveAsync(warningFileName, warningBytes, cancellationToken);
            warningContentType = "text/csv";
            warningFile = new FileExportResultDto
            {
                FileName = warningFileName,
                ContentType = warningContentType,
                ContentBase64 = Convert.ToBase64String(warningBytes)
            };
        }

        var report = new StatutoryReport
        {
            Id = Guid.NewGuid(),
            Type = StatutoryReportType.EpfEtf,
            PayRunId = payRun.Id,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            GeneratedAtUtc = generatedAt,
            GeneratedBy = _currentUserService.UserName ?? "system",
            Status = StatutoryReportStatus.Generated,
            FilePath = filePath,
            FileName = fileName,
            ContentType = exporter.ContentType,
            Checksum = ComputeChecksum(bytes),
            WarningCount = warnings.Count,
            WarningFilePath = warningFilePath,
            WarningFileName = warningFileName,
            WarningContentType = warningContentType
        };

        await _dbContext.StatutoryReports.AddAsync(report, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EpfEtfReportResultDto
        {
            ReportId = report.Id,
            PayRunId = payRun.Id,
            PayRunCode = payRun.Code,
            PayRunName = payRun.Name,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate,
            EmployeeCount = summary.EmployeeCount,
            ContributableBase = summary.ContributableBaseTotal,
            EmployeeEpfTotal = summary.EmployeeEpfTotal,
            EmployerEpfTotal = summary.EmployerEpfTotal,
            EmployerEtfTotal = summary.EmployerEtfTotal,
            File = new FileExportResultDto
            {
                FileName = fileName,
                ContentType = exporter.ContentType,
                ContentBase64 = Convert.ToBase64String(bytes)
            },
            WarningFile = warningFile,
            Employees = employeeDtos,
            Warnings = warnings
        };
    }

    public async Task<IReadOnlyList<StatutoryReportHistoryDto>> GetReportsAsync(
        StatutoryReportType? type = null,
        Guid? payRunId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StatutoryReports
            .AsNoTracking()
            .Include(r => r.PayRun)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        if (payRunId.HasValue)
        {
            query = query.Where(r => r.PayRunId == payRunId.Value);
        }

        var reports = await query
            .OrderByDescending(r => r.GeneratedAtUtc)
            .ToListAsync(cancellationToken);

        return reports.Select(report => new StatutoryReportHistoryDto
        {
            Id = report.Id,
            Type = report.Type,
            PayRunId = report.PayRunId,
            PayRunCode = report.PayRun?.Code ?? string.Empty,
            PayRunName = report.PayRun?.Name ?? string.Empty,
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            GeneratedAtUtc = report.GeneratedAtUtc,
            GeneratedBy = report.GeneratedBy,
            Status = report.Status,
            FileName = report.FileName,
            WarningCount = report.WarningCount
        }).ToList();
    }

    public async Task<FileExportResultDto?> DownloadReportAsync(
        Guid reportId,
        bool includeWarnings = false,
        CancellationToken cancellationToken = default)
    {
        var report = await _dbContext.StatutoryReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report is null)
        {
            return null;
        }

        var filePath = includeWarnings ? report.WarningFilePath : report.FilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var bytes = await _storage.ReadAsync(filePath, cancellationToken);

        return new FileExportResultDto
        {
            FileName = includeWarnings ? report.WarningFileName ?? "statutory-warnings.csv" : report.FileName,
            ContentType = includeWarnings ? report.WarningContentType ?? "text/csv" : report.ContentType,
            ContentBase64 = Convert.ToBase64String(bytes)
        };
    }

    private static decimal RoundCurrency(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static string ComputeChecksum(byte[] bytes)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static string BuildWarningCsv(IEnumerable<EpfEtfReportWarningDto> warnings)
    {
        var builder = new StringBuilder();
        builder.AppendLine("EmployeeCode,EmployeeName,Message");

        foreach (var warning in warnings)
        {
            builder.AppendLine(string.Join(",",
                Escape(warning.EmployeeCode),
                Escape(warning.EmployeeName),
                Escape(warning.Message)));
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sanitized = value.Replace("\"", "\"\"");
        return $"\"{sanitized}\"";
    }
}
