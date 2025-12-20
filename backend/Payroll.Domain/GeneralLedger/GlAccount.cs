using Payroll.Domain.Common;

namespace Payroll.Domain.GeneralLedger;

public class GlAccount : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GlAccountType Type { get; set; }
    public bool IsActive { get; set; } = true;
}
