using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.PayrollConfig.DTOs;

public class CreateTaxRuleSetRequest
{
    public string Name { get; set; } = null!;
    public int YearOfAssessment { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public TaxRuleSetFrequency Frequency { get; set; } = TaxRuleSetFrequency.Monthly;

    public List<CreateTaxSlabItem> Slabs { get; set; } = new();
    public List<CreateTaxReliefItem> Reliefs { get; set; } = new();
}

public class CreateTaxSlabItem
{
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
    public int Order { get; set; }
}

public class CreateTaxReliefItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TaxReliefType ReliefType { get; set; }
    public TaxReliefFrequency Frequency { get; set; }
}
