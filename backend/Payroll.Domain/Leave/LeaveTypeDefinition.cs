using Payroll.Domain.Common;

namespace Payroll.Domain.Leave;

public class LeaveTypeDefinition : AuditableEntity
{
    public LeaveTypeCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPaid { get; set; } = true;
    public bool AllowsHalfDay { get; set; } = true;
    public bool Encashable { get; set; }
    public decimal EncashmentRateMultiplier { get; set; } = 1m;
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
