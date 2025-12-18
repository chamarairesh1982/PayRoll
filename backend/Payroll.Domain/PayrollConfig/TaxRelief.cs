using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class TaxRelief : AuditableEntity
{
    public Guid TaxRuleSetId { get; set; }
    public TaxRuleSet TaxRuleSet { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TaxReliefType ReliefType { get; set; }
    public TaxReliefFrequency Frequency { get; set; }
}

public enum TaxReliefType
{
    IncomeRelief = 1,
    TaxRebate = 2
}

public enum TaxReliefFrequency
{
    Monthly = 1,
    Annual = 2
}
