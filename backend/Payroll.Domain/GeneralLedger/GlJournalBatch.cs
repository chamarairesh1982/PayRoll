using Payroll.Domain.Common;
using Payroll.Domain.Payroll;

namespace Payroll.Domain.GeneralLedger;

public class GlJournalBatch : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public PayRun? PayRun { get; set; }
    public GlJournalBatchStatus Status { get; set; } = GlJournalBatchStatus.Draft;
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ExportedAtUtc { get; set; }
    public string? ExportedByUserId { get; set; }
    public string? ExportedByUserName { get; set; }
    public string? Notes { get; set; }
    public string? ChecksumSha256 { get; set; }
    public ICollection<GlJournalLine> Lines { get; set; } = new List<GlJournalLine>();
}
