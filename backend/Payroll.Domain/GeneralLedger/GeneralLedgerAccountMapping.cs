using Payroll.Domain.Common;

namespace Payroll.Domain.GeneralLedger;

public class GeneralLedgerAccountMapping : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GeneralLedgerMappingType MappingType { get; set; }
    public string DebitAccount { get; set; } = string.Empty;
    public string CreditAccount { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
