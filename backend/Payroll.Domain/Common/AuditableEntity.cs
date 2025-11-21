namespace Payroll.Domain.Common;

public abstract class AuditableEntity : EntityBase
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;
}
