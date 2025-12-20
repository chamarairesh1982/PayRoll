using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class BankExportTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BankExportFormat Format { get; set; }
    public bool IsActive { get; set; }
    public string? Delimiter { get; set; }
    public int HeaderRowCount { get; set; }
}

public class BankExportGenerateRequest
{
    public Guid TemplateId { get; set; }
}

public class BankExportValidationErrorDto
{
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PayRunBankExportDto
{
    public Guid Id { get; set; }
    public Guid PayRunId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public BankExportStatus Status { get; set; }
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public DateTime? DownloadedAtUtc { get; set; }
    public string? DownloadedByUserId { get; set; }
    public string? DownloadedByUserName { get; set; }
    public string? FileName { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? ErrorSummary { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int ErrorCount { get; set; }
}

public class BankExportGenerateResultDto
{
    public PayRunBankExportDto Export { get; set; } = new();
    public List<BankExportValidationErrorDto> ValidationErrors { get; set; } = new();
}
