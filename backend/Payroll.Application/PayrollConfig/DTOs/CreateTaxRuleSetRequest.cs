namespace Payroll.Application.PayrollConfig.DTOs;

public class CreateTaxRuleSetRequest
{
    public string Name { get; set; } = null!;
    public int YearOfAssessment { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }

    public List<CreateTaxSlabItem> Slabs { get; set; } = new();
}

public class CreateTaxSlabItem
{
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal RatePercent { get; set; }
    public int Order { get; set; }
}
