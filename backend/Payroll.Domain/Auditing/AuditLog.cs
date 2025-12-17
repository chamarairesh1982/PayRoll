using Payroll.Domain.Common;

namespace Payroll.Domain.Auditing;

public class AuditLog : EntityBase
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string BeforeSnapshot { get; set; } = string.Empty;
    public string AfterSnapshot { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
