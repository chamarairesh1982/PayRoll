using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.PayrollConfig.DTOs;

public class CreateTaxSlabRequest
{
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
    public int Order { get; set; }
}

public class UpdateTaxSlabRequest
{
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
    public int Order { get; set; }
}

public class CreateTaxReliefRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TaxReliefType ReliefType { get; set; }
    public TaxReliefFrequency Frequency { get; set; }
}

public class UpdateTaxReliefRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TaxReliefType ReliefType { get; set; }
    public TaxReliefFrequency Frequency { get; set; }
}
