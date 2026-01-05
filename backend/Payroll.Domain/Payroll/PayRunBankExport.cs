using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PayRunBankExport : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public PayRun? PayRun { get; set; }
    public Guid TemplateId { get; set; }
    public BankExportTemplate? Template { get; set; }
    public BankExportStatus Status { get; set; } = BankExportStatus.Pending;
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public DateTime? DownloadedAtUtc { get; set; }
    public string? DownloadedByUserId { get; set; }
    public string? DownloadedByUserName { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? ErrorSummary { get; set; }
    public ICollection<PayRunBankExportError> Errors { get; set; } = new List<PayRunBankExportError>();
}
