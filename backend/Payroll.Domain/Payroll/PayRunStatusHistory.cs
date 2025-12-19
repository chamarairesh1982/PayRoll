using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PayRunStatusHistory : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public PayRun PayRun { get; set; } = null!;
    public PayRunStatus FromStatus { get; set; }
    public PayRunStatus ToStatus { get; set; }
    public string? ActorUserId { get; set; }
    public string? ActorDisplayName { get; set; }
    public string? Comment { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? PreviousHash { get; set; }
    public string Hash { get; set; } = string.Empty;
}
