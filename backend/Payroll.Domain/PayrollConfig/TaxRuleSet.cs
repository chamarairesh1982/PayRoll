using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class TaxRuleSet : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public int YearOfAssessment { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TaxSlab> Slabs { get; set; } = new List<TaxSlab>();
}
