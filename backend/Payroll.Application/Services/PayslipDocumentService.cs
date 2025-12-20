using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Exceptions;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Payroll.Application.Services;

public class PayslipDocumentService : IPayslipDocumentService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly IPayslipDocumentStorage _storage;
    private readonly ICurrentUserService _currentUserService;

    public PayslipDocumentService(
        IPayrollDbContext dbContext,
        IPayslipDocumentStorage storage,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _storage = storage;
        _currentUserService = currentUserService;
    }

    public async Task<PayslipDocumentDto?> GenerateAsync(Guid payRunId, Guid employeeId, bool regenerate, CancellationToken cancellationToken = default)
    {
        EnsurePayslipRole("generate payslip documents");

        var payRun = await LoadPayRunAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            return null;
        }

        EnsurePayRunLocked(payRun);

        var paySlip = payRun.PaySlips.FirstOrDefault(ps => ps.EmployeeId == employeeId);
        if (paySlip is null)
        {
            return null;
        }

        var existing = await _dbContext.PayslipDocuments
            .AsNoTracking()
            .Include(document => document.Employee)
            .Where(document => document.PayRunId == payRunId
                && document.EmployeeId == employeeId
                && document.Status == PayslipDocumentStatus.Generated)
            .OrderByDescending(document => document.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null && !regenerate)
        {
            return MapDocument(existing);
        }

        var now = DateTime.UtcNow;
        var actor = GetActor();
        var document = new PayslipDocument
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            EmployeeId = employeeId,
            Status = PayslipDocumentStatus.Pending,
            GeneratedAtUtc = now,
            GeneratedByUserId = actor.UserId,
            GeneratedByUserName = actor.UserName,
            CreatedBy = actor.UserName
        };

        _dbContext.PayslipDocuments.Add(document);

        try
        {
            var payload = BuildPayload(payRun, paySlip);
            var checksum = ComputeChecksum(payload);
            var pdfBytes = BuildPaySlipPdf(payRun, paySlip, payload, checksum, now);
            var version = await GetNextVersionAsync(payRunId, employeeId, cancellationToken);
            var timestamp = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var employeeCode = paySlip.Employee?.EmployeeCode ?? employeeId.ToString("N");
            var fileName = $"Payslip-{employeeCode}-{payRun.PayDate:yyyyMMdd}-v{version}-{timestamp}.pdf";
            var filePath = await _storage.SaveAsync(fileName, pdfBytes, cancellationToken);

            document.Status = PayslipDocumentStatus.Generated;
            document.FileName = fileName;
            document.FilePath = filePath;
            document.ChecksumSha256 = checksum;
            document.ErrorSummary = null;
        }
        catch (Exception ex)
        {
            document.Status = PayslipDocumentStatus.Failed;
            document.ErrorSummary = ex.Message;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        document.Employee = paySlip.Employee;
        return MapDocument(document);
    }

    public async Task<PayslipBulkGenerateResultDto> GenerateBulkAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        EnsurePayslipRole("generate payslip documents");

        var payRun = await LoadPayRunAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found.");
        }

        EnsurePayRunLocked(payRun);

        var generated = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var paySlip in payRun.PaySlips)
        {
            var existing = await _dbContext.PayslipDocuments
                .AsNoTracking()
                .Where(document => document.PayRunId == payRunId
                    && document.EmployeeId == paySlip.EmployeeId
                    && document.Status == PayslipDocumentStatus.Generated)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is not null)
            {
                skipped++;
                continue;
            }

            var result = await GenerateAsync(payRunId, paySlip.EmployeeId, false, cancellationToken);
            if (result is null || result.Status == PayslipDocumentStatus.Failed)
            {
                failed++;
                continue;
            }

            generated++;
        }

        return new PayslipBulkGenerateResultDto
        {
            GeneratedCount = generated,
            SkippedCount = skipped,
            FailedCount = failed
        };
    }

    public async Task<IReadOnlyList<PayslipDocumentDto>> GetDocumentsForPayRunAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        EnsurePayslipRole("view payslip documents");

        return await _dbContext.PayslipDocuments
            .AsNoTracking()
            .Include(document => document.Employee)
            .Where(document => document.PayRunId == payRunId)
            .OrderByDescending(document => document.CreatedAt)
            .Select(document => new PayslipDocumentDto
            {
                Id = document.Id,
                PayRunId = document.PayRunId,
                EmployeeId = document.EmployeeId,
                EmployeeCode = document.Employee != null ? document.Employee.EmployeeCode : null,
                EmployeeName = document.Employee != null ? document.Employee.FullName : null,
                Status = document.Status,
                GeneratedAtUtc = document.GeneratedAtUtc,
                GeneratedByUserId = document.GeneratedByUserId,
                GeneratedByUserName = document.GeneratedByUserName,
                FileName = document.FileName,
                ChecksumSha256 = document.ChecksumSha256,
                ErrorSummary = document.ErrorSummary
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FileExportResultDto?> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        EnsurePayslipRole("download payslip documents");

        var document = await _dbContext.PayslipDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        if (document.Status != PayslipDocumentStatus.Generated)
        {
            throw new InvalidOperationException("Payslip document is not available.");
        }

        if (string.IsNullOrWhiteSpace(document.FilePath) || string.IsNullOrWhiteSpace(document.FileName))
        {
            throw new InvalidOperationException("Payslip document file path is missing.");
        }

        var bytes = await _storage.ReadAsync(document.FilePath, cancellationToken);

        return new FileExportResultDto
        {
            FileName = document.FileName,
            ContentType = "application/pdf",
            ContentBase64 = Convert.ToBase64String(bytes)
        };
    }

    private async Task<PayRun?> LoadPayRunAsync(Guid payRunId, CancellationToken cancellationToken)
    {
        return await _dbContext.PayRuns
            .Include(pr => pr.Company)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);
    }

    private static PayslipDocumentPayload BuildPayload(PayRun payRun, PaySlip paySlip)
    {
        return new PayslipDocumentPayload
        {
            PayRunId = payRun.Id,
            EmployeeId = paySlip.EmployeeId,
            EmployeeCode = paySlip.Employee?.EmployeeCode,
            EmployeeName = paySlip.Employee?.FullName,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate,
            GrossPay = paySlip.TotalEarnings,
            TotalDeductions = paySlip.TotalDeductions,
            NetPay = paySlip.NetPay,
            EmployeeEpf = paySlip.EmployeeEpf,
            EmployerEpf = paySlip.EmployerEpf,
            EmployerEtf = paySlip.EmployerEtf,
            PayeTax = paySlip.PayeTax,
            Earnings = paySlip.Earnings
                .OrderBy(e => e.Code)
                .Select(e => new PayslipDocumentLine
                {
                    Code = e.Code,
                    Description = e.Description,
                    Amount = e.Amount
                })
                .ToList(),
            Deductions = paySlip.Deductions
                .OrderBy(d => d.Code)
                .Select(d => new PayslipDocumentLine
                {
                    Code = d.Code,
                    Description = d.Description,
                    Amount = d.Amount
                })
                .ToList()
        };
    }

    private static string ComputeChecksum(PayslipDocumentPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static byte[] BuildPaySlipPdf(
        PayRun payRun,
        PaySlip paySlip,
        PayslipDocumentPayload payload,
        string authenticityHash,
        DateTime generatedAtUtc)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var companyName = payRun.Company?.Name ?? "Company Name";
        var companyAddress = "Company Address";
        var earnings = payload.Earnings;
        var deductions = payload.Deductions;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(companyName).FontSize(14).SemiBold();
                        column.Item().Text(companyAddress).FontSize(9).FontColor(Colors.Grey.Darken1);
                        column.Item().Text($"Payslip - {payRun.Name}").FontSize(12).Bold();
                        column.Item().Text($"Period: {payRun.PeriodStart:yyyy-MM-dd} to {payRun.PeriodEnd:yyyy-MM-dd}");
                        column.Item().Text($"Pay Date: {payRun.PayDate:yyyy-MM-dd}");
                    });

                    row.ConstantItem(220).Column(column =>
                    {
                        column.Item().Text("Employee Details").Bold();
                        column.Item().Text($"Name: {paySlip.Employee?.FullName ?? "N/A"}");
                        column.Item().Text($"Code: {paySlip.Employee?.EmployeeCode ?? "-"}");
                        column.Item().Text($"NIC: {paySlip.Employee?.NicNumber ?? "-"}");
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Earnings").Bold();
                            inner.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").SemiBold();
                                    header.Cell().Text("Description").SemiBold();
                                    header.Cell().Text("Amount").SemiBold().AlignRight();
                                });

                                foreach (var earning in earnings)
                                {
                                    table.Cell().Text(earning.Code);
                                    table.Cell().Text(earning.Description);
                                    table.Cell().AlignRight().Text(earning.Amount.ToString("N2"));
                                }

                                table.Cell().ColumnSpan(2).Text("Total Earnings").SemiBold();
                                table.Cell().AlignRight().Text(payload.GrossPay.ToString("N2")).SemiBold();
                            });
                        });

                        row.Spacing(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Deductions").Bold();
                            inner.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").SemiBold();
                                    header.Cell().Text("Description").SemiBold();
                                    header.Cell().Text("Amount").SemiBold().AlignRight();
                                });

                                foreach (var deduction in deductions)
                                {
                                    table.Cell().Text(deduction.Code);
                                    table.Cell().Text(deduction.Description);
                                    table.Cell().AlignRight().Text($"-{deduction.Amount:N2}");
                                }

                                table.Cell().ColumnSpan(2).Text("Total Deductions").SemiBold();
                                table.Cell().AlignRight().Text($"-{payload.TotalDeductions:N2}").SemiBold();
                            });
                        });
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Summary").Bold();
                            inner.Item().Text($"Basic Salary: {paySlip.BasicSalary:N2}");
                            inner.Item().Text($"Total Earnings: {payload.GrossPay:N2}");
                            inner.Item().Text($"Total Deductions: {payload.TotalDeductions:N2}");
                            inner.Item().Text($"Net Pay: {payload.NetPay:N2}").FontSize(12).Bold();
                        });

                        row.Spacing(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Statutory Contributions").Bold();
                            inner.Item().Text($"Employee EPF: {payload.EmployeeEpf:N2}");
                            inner.Item().Text($"Employer EPF: {payload.EmployerEpf:N2}");
                            inner.Item().Text($"Employer ETF: {payload.EmployerEtf:N2}");
                            inner.Item().Text($"PAYE/APIT: {payload.PayeTax:N2}");
                        });
                    });
                });

                page.Footer().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().Text($"Generated at {generatedAtUtc:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().Text($"PayRun: {payRun.Id} | Employee: {paySlip.EmployeeId}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().Text($"Authenticity hash: {authenticityHash}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    private static PayslipDocumentDto MapDocument(PayslipDocument document)
    {
        return new PayslipDocumentDto
        {
            Id = document.Id,
            PayRunId = document.PayRunId,
            EmployeeId = document.EmployeeId,
            EmployeeCode = document.Employee?.EmployeeCode,
            EmployeeName = document.Employee?.FullName,
            Status = document.Status,
            GeneratedAtUtc = document.GeneratedAtUtc,
            GeneratedByUserId = document.GeneratedByUserId,
            GeneratedByUserName = document.GeneratedByUserName,
            FileName = document.FileName,
            ChecksumSha256 = document.ChecksumSha256,
            ErrorSummary = document.ErrorSummary
        };
    }

    private static void EnsurePayRunLocked(PayRun payRun)
    {
        if (payRun.Status != PayRunStatus.Locked && !payRun.IsLocked)
        {
            throw new InvalidOperationException("Payslips can only be generated after the pay run is locked.");
        }
    }

    private async Task<int> GetNextVersionAsync(Guid payRunId, Guid employeeId, CancellationToken cancellationToken)
    {
        var count = await _dbContext.PayslipDocuments
            .AsNoTracking()
            .CountAsync(document => document.PayRunId == payRunId && document.EmployeeId == employeeId, cancellationToken);

        return count + 1;
    }

    private void EnsurePayslipRole(string action)
    {
        var hasRole = _currentUserService.Roles.Any(role =>
            string.Equals(role, "Approver", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "PayrollAdmin", StringComparison.OrdinalIgnoreCase));

        if (!hasRole)
        {
            throw new ForbiddenAccessException($"Only payroll approvers or admins can {action}.");
        }
    }

    private (string UserId, string UserName) GetActor()
    {
        var userName = string.IsNullOrWhiteSpace(_currentUserService.UserName) ? "Unknown" : _currentUserService.UserName!;
        var userId = _currentUserService.UserId ?? string.Empty;
        return (userId, userName);
    }
}
