namespace Payroll.Application.PayrollConfig.DTOs;

public class TaxRuleSetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int YearOfAssessment { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    public List<TaxSlabDto> Slabs { get; set; } = new();
}
