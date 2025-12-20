using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.BankExports;
using Payroll.Application.DTOs;
using Payroll.Application.Exceptions;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Services;

public class BankExportService : IBankExportService
{
    private const int MinAccountLength = 8;
    private const int MaxAccountLength = 20;
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBankExportStorage _storage;
    private readonly BankExportTemplateResolver _templateResolver = new();

    public BankExportService(
        IPayrollDbContext dbContext,
        ICurrentUserService currentUserService,
        IBankExportStorage storage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _storage = storage;
    }

    public async Task<IReadOnlyList<BankExportTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        EnsureBankExportRole("view bank export templates");
        return await _dbContext.BankExportTemplates
            .AsNoTracking()
            .Where(template => template.IsActive)
            .OrderBy(template => template.Name)
            .Select(template => new BankExportTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Format = template.Format,
                IsActive = template.IsActive,
                Delimiter = template.Delimiter,
                HeaderRowCount = template.HeaderRowCount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PayRunBankExportDto>> GetExportsAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        EnsureBankExportRole("view bank export history");

        return await _dbContext.PayRunBankExports
            .AsNoTracking()
            .Include(export => export.Template)
            .Where(export => export.PayRunId == payRunId)
            .OrderByDescending(export => export.CreatedAt)
            .Select(export => new PayRunBankExportDto
            {
                Id = export.Id,
                PayRunId = export.PayRunId,
                TemplateId = export.TemplateId,
                TemplateName = export.Template != null ? export.Template.Name : string.Empty,
                Status = export.Status,
                GeneratedAtUtc = export.GeneratedAtUtc,
                GeneratedByUserId = export.GeneratedByUserId,
                GeneratedByUserName = export.GeneratedByUserName,
                DownloadedAtUtc = export.DownloadedAtUtc,
                DownloadedByUserId = export.DownloadedByUserId,
                DownloadedByUserName = export.DownloadedByUserName,
                FileName = export.FileName,
                ChecksumSha256 = export.ChecksumSha256,
                ErrorSummary = export.ErrorSummary,
                CreatedAtUtc = export.CreatedAt,
                ErrorCount = export.Errors.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BankExportGenerateResultDto> GenerateExportAsync(
        Guid payRunId,
        BankExportGenerateRequest request,
        bool regenerate,
        CancellationToken cancellationToken = default)
    {
        EnsureBankExportRole("generate bank exports");

        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found.");
        }

        if (payRun.Status != PayRunStatus.Locked && !payRun.IsLocked)
        {
            throw new InvalidOperationException("Bank exports can only be generated for locked pay runs.");
        }

        var template = await _dbContext.BankExportTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template is null || !template.IsActive)
        {
            throw new InvalidOperationException("Bank export template is unavailable.");
        }

        var exportTemplate = _templateResolver.Resolve(template.Name);

        var existingGenerated = await _dbContext.PayRunBankExports
            .Include(export => export.Template)
            .FirstOrDefaultAsync(export =>
                export.PayRunId == payRunId &&
                export.TemplateId == template.Id &&
                export.Status == BankExportStatus.Generated,
                cancellationToken);

        if (existingGenerated is not null && !regenerate)
        {
            return new BankExportGenerateResultDto
            {
                Export = MapExport(existingGenerated)
            };
        }

        var now = DateTime.UtcNow;
        var actor = GetActor();
        var exportRecord = new PayRunBankExport
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            TemplateId = template.Id,
            Status = BankExportStatus.Pending,
            CreatedBy = actor.UserName,
            GeneratedByUserId = actor.UserId,
            GeneratedByUserName = actor.UserName,
            GeneratedAtUtc = now
        };

        var validationErrors = ValidatePayRun(payRun);

        if (validationErrors.Any())
        {
            exportRecord.Status = BankExportStatus.Failed;
            exportRecord.ErrorSummary = $"Validation failed for {validationErrors.Count} entries.";

            foreach (var failure in validationErrors)
            {
                exportRecord.Errors.Add(new PayRunBankExportError
                {
                    Id = Guid.NewGuid(),
                    PayRunBankExportId = exportRecord.Id,
                    EmployeeId = failure.EmployeeId,
                    EmployeeCode = failure.EmployeeCode,
                    Field = failure.Field,
                    Message = failure.Message,
                    CreatedBy = actor.UserName
                });
            }

            _dbContext.PayRunBankExports.Add(exportRecord);
            payRun.ExportStatus = BankExportStatus.Failed;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BankExportGenerateResultDto
            {
                Export = MapExport(exportRecord, template.Name),
                ValidationErrors = validationErrors
            };
        }

        var rows = BuildRows(payRun);
        var content = exportTemplate.Render(rows);
        var bytes = Encoding.UTF8.GetBytes(content);
        var checksum = ComputeChecksum(bytes);

        var version = await GetNextVersionAsync(payRunId, template.Id, cancellationToken);
        var timestamp = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var fileName = $"{payRun.Code}-{template.Name}-v{version}-{timestamp}.{exportTemplate.FileExtension}";
        var filePath = await _storage.SaveAsync(fileName, bytes, cancellationToken);

        exportRecord.Status = BankExportStatus.Generated;
        exportRecord.FileName = fileName;
        exportRecord.FilePath = filePath;
        exportRecord.ChecksumSha256 = checksum;
        exportRecord.ErrorSummary = null;

        _dbContext.PayRunBankExports.Add(exportRecord);

        payRun.ExportStatus = BankExportStatus.Generated;
        payRun.ExportedBank = template.Name;
        payRun.ExportedAt = now;
        payRun.ExportDownloadedAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BankExportGenerateResultDto
        {
            Export = MapExport(exportRecord, template.Name),
            ValidationErrors = validationErrors
        };
    }

    public async Task<FileExportResultDto?> DownloadExportAsync(Guid exportId, CancellationToken cancellationToken = default)
    {
        EnsureBankExportRole("download bank exports");

        var export = await _dbContext.PayRunBankExports
            .Include(e => e.PayRun)
            .Include(e => e.Template)
            .FirstOrDefaultAsync(e => e.Id == exportId, cancellationToken);

        if (export is null)
        {
            return null;
        }

        if (export.Status != BankExportStatus.Generated && export.Status != BankExportStatus.Downloaded)
        {
            throw new InvalidOperationException("Bank export file is not available.");
        }

        if (string.IsNullOrWhiteSpace(export.FilePath) || string.IsNullOrWhiteSpace(export.FileName))
        {
            throw new InvalidOperationException("Bank export file path is missing.");
        }

        var bytes = await _storage.ReadAsync(export.FilePath, cancellationToken);

        if (export.Status == BankExportStatus.Generated)
        {
            var actor = GetActor();
            export.Status = BankExportStatus.Downloaded;
            export.DownloadedAtUtc = DateTime.UtcNow;
            export.DownloadedByUserId = actor.UserId;
            export.DownloadedByUserName = actor.UserName;

            if (export.PayRun != null)
            {
                export.PayRun.ExportStatus = BankExportStatus.Downloaded;
                export.PayRun.ExportDownloadedAt = export.DownloadedAtUtc;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new FileExportResultDto
        {
            FileName = export.FileName,
            ContentType = export.Template != null
                ? _templateResolver.Resolve(export.Template.Name).ContentType
                : "text/plain",
            ContentBase64 = Convert.ToBase64String(bytes)
        };
    }

    public async Task<FileExportResultDto?> DownloadErrorsAsync(Guid exportId, CancellationToken cancellationToken = default)
    {
        EnsureBankExportRole("download bank export errors");

        var export = await _dbContext.PayRunBankExports
            .Include(e => e.PayRun)
            .Include(e => e.Errors)
            .FirstOrDefaultAsync(e => e.Id == exportId, cancellationToken);

        if (export is null)
        {
            return null;
        }

        var builder = new StringBuilder();
        builder.AppendLine("EmployeeId,EmployeeCode,Field,Message");

        foreach (var error in export.Errors.OrderBy(e => e.EmployeeCode))
        {
            builder.Append(error.EmployeeId?.ToString() ?? string.Empty).Append(',');
            builder.Append(error.EmployeeCode ?? string.Empty).Append(',');
            builder.Append('"').Append(error.Field.Replace("\"", "''")).Append('"').Append(',');
            builder.Append('"').Append(error.Message.Replace("\"", "''")).Append('"');
            builder.AppendLine();
        }

        var content = builder.ToString();
        var bytes = Encoding.UTF8.GetBytes(content);
        var fileName = $"BankExportErrors-{export.PayRun?.Code ?? export.Id.ToString()}-{export.Id}.csv";

        return new FileExportResultDto
        {
            FileName = fileName,
            ContentType = "text/csv",
            ContentBase64 = Convert.ToBase64String(bytes)
        };
    }

    private static List<BankExportRow> BuildRows(PayRun payRun)
    {
        var rows = new List<BankExportRow>();
        var reference = string.IsNullOrWhiteSpace(payRun.Reference) ? payRun.Code : payRun.Reference;
        var rowNo = 1;

        foreach (var paySlip in payRun.PaySlips)
        {
            var employee = paySlip.Employee;
            if (employee is null)
            {
                continue;
            }

            rows.Add(new BankExportRow(
                rowNo++,
                employee.FullName,
                employee.BankAccountNumber ?? string.Empty,
                paySlip.NetPay,
                reference,
                employee.EmployeeCode));
        }

        return rows;
    }

    private static string ComputeChecksum(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private List<BankExportValidationErrorDto> ValidatePayRun(PayRun payRun)
    {
        var failures = new List<BankExportValidationErrorDto>();

        foreach (var paySlip in payRun.PaySlips)
        {
            var employee = paySlip.Employee;

            if (employee is null)
            {
                failures.Add(new BankExportValidationErrorDto
                {
                    EmployeeId = paySlip.EmployeeId,
                    Field = "employee",
                    Message = "Employee details unavailable."
                });
                continue;
            }

            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                failures.Add(new BankExportValidationErrorDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    Field = "beneficiaryName",
                    Message = "Employee name is required."
                });
            }

            if (paySlip.NetPay <= 0)
            {
                failures.Add(new BankExportValidationErrorDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    Field = "netPay",
                    Message = "Net pay must be greater than zero for bank export."
                });
            }

            var accountNumber = employee.BankAccountNumber;
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                failures.Add(new BankExportValidationErrorDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    Field = "accountNumber",
                    Message = "Bank account number is required."
                });
            }
            else
            {
                if (accountNumber.Length < MinAccountLength || accountNumber.Length > MaxAccountLength)
                {
                    failures.Add(new BankExportValidationErrorDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        Field = "accountNumber",
                        Message = $"Bank account number must be {MinAccountLength}-{MaxAccountLength} digits."
                    });
                }

                if (!accountNumber.All(char.IsDigit))
                {
                    failures.Add(new BankExportValidationErrorDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        Field = "accountNumber",
                        Message = "Bank account number must contain only digits."
                    });
                }
            }
        }

        return failures;
    }

    private PayRunBankExportDto MapExport(PayRunBankExport export, string? templateNameOverride = null)
    {
        return new PayRunBankExportDto
        {
            Id = export.Id,
            PayRunId = export.PayRunId,
            TemplateId = export.TemplateId,
            TemplateName = templateNameOverride ?? export.Template?.Name ?? string.Empty,
            Status = export.Status,
            GeneratedAtUtc = export.GeneratedAtUtc,
            GeneratedByUserId = export.GeneratedByUserId,
            GeneratedByUserName = export.GeneratedByUserName,
            DownloadedAtUtc = export.DownloadedAtUtc,
            DownloadedByUserId = export.DownloadedByUserId,
            DownloadedByUserName = export.DownloadedByUserName,
            FileName = export.FileName,
            ChecksumSha256 = export.ChecksumSha256,
            ErrorSummary = export.ErrorSummary,
            CreatedAtUtc = export.CreatedAt,
            ErrorCount = export.Errors.Count
        };
    }

    private async Task<int> GetNextVersionAsync(Guid payRunId, Guid templateId, CancellationToken cancellationToken)
    {
        var count = await _dbContext.PayRunBankExports
            .AsNoTracking()
            .CountAsync(export => export.PayRunId == payRunId && export.TemplateId == templateId, cancellationToken);

        return count + 1;
    }

    private void EnsureBankExportRole(string action)
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
