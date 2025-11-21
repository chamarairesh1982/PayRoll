using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class TaxSlab : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid TaxRuleSetId { get; set; }
    public TaxRuleSet TaxRuleSet { get; set; } = null!;

    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal RatePercent { get; set; }

    public int Order { get; set; }
}
