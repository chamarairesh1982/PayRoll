namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateTaxRuleSetRequest
{
    public string? Name { get; set; }
    public int? YearOfAssessment { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }

    public List<UpdateTaxSlabItem>? Slabs { get; set; }
}

public class UpdateTaxSlabItem
{
    public Guid? Id { get; set; }
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal RatePercent { get; set; }
    public int Order { get; set; }
}
