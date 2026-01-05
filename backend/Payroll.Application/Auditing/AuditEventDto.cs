using Payroll.Domain.Auditing;

namespace Payroll.Application.Auditing;

public class AuditEventDto
{
    public Guid Id { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string ActorUserId { get; init; } = string.Empty;
    public string? ActorDisplayName { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? BeforeJson { get; init; }
    public string? AfterJson { get; init; }
    public string? CorrelationId { get; init; }

    public static AuditEventDto FromEntity(AuditEvent entity) => new()
    {
        Id = entity.Id,
        TimestampUtc = entity.TimestampUtc,
        ActorUserId = entity.ActorUserId,
        ActorDisplayName = entity.ActorDisplayName,
        EntityType = entity.EntityType,
        EntityId = entity.EntityId,
        Action = entity.Action,
        BeforeJson = entity.BeforeJson,
        AfterJson = entity.AfterJson,
        CorrelationId = entity.CorrelationId
    };
}
