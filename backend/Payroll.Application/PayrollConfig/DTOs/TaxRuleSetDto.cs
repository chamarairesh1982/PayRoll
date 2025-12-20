using Payroll.Domain.PayrollConfig;

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
    public TaxRuleSetFrequency Frequency { get; set; }

    public List<TaxSlabDto> Slabs { get; set; } = new();
    public List<TaxReliefDto> Reliefs { get; set; } = new();
}

public class TaxReliefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TaxReliefType ReliefType { get; set; }
    public TaxReliefFrequency Frequency { get; set; }
}
