using Payroll.Domain.Auditing;

namespace Payroll.Application.Auditing;

public class AuditLogDto
{
    public Guid Id { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string BeforeSnapshot { get; init; } = string.Empty;
    public string AfterSnapshot { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;

    public static AuditLogDto FromEntity(AuditLog entity) => new()
    {
        Id = entity.Id,
        EntityName = entity.EntityName,
        EntityId = entity.EntityId,
        Action = entity.Action,
        BeforeSnapshot = entity.BeforeSnapshot,
        AfterSnapshot = entity.AfterSnapshot,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy
    };
}
