using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class EpfEtfRuleSet : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public decimal EmployeeEpfRate { get; set; }
    public decimal EmployerEpfRate { get; set; }
    public decimal EmployerEtfRate { get; set; }

    public decimal? MinimumWageForEpf { get; set; }
    public decimal? MaximumEarningForEpf { get; set; }
    public decimal? MaximumEarningForEtf { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
