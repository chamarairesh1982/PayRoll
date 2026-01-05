using Payroll.Domain.Common;

namespace Payroll.Domain.Auditing;

public class AuditEvent : EntityBase
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string ActorUserId { get; set; } = string.Empty;
    public string? ActorDisplayName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? CorrelationId { get; set; }
    public string? PreviousHash { get; set; }
    public string Hash { get; set; } = string.Empty;
}
