using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class Bank : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public ICollection<BankBranch> Branches { get; set; } = new List<BankBranch>();
}
