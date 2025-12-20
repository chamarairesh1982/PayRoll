using Payroll.Domain.Common;
using Payroll.Domain.Organizations;

namespace Payroll.Domain.GeneralLedger;

public class GlMapping : AuditableEntity
{
    public string PayComponentCode { get; set; } = string.Empty;
    public GlPayComponentType PayComponentType { get; set; }
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public Guid? DebitAccountId { get; set; }
    public GlAccount? DebitAccount { get; set; }
    public Guid? CreditAccountId { get; set; }
    public GlAccount? CreditAccount { get; set; }
    public GlPostingSideRule PostingSideRule { get; set; } = GlPostingSideRule.DebitWhenPositive;
    public string? Notes { get; set; }
}
