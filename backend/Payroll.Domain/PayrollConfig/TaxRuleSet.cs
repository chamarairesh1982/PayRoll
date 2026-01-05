using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class TaxRuleSet : AuditableEntity
{
    public string Name { get; set; } = null!;
    public int YearOfAssessment { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public TaxRuleSetFrequency Frequency { get; set; } = TaxRuleSetFrequency.Monthly;

    public bool IsDefault { get; set; }

    public ICollection<TaxSlab> Slabs { get; set; } = new List<TaxSlab>();
    public ICollection<TaxRelief> Reliefs { get; set; } = new List<TaxRelief>();
}

public enum TaxRuleSetFrequency
{
    Monthly = 1
}
