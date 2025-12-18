using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PayRunApproval : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public PayRun PayRun { get; set; } = null!;
    public PayRunStatus FromStatus { get; set; }
    public PayRunStatus ToStatus { get; set; }
    public string? ActorUserId { get; set; }
    public string ActorUserName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime ActionedAt { get; set; } = DateTime.UtcNow;
}
