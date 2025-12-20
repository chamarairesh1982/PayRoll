using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class BankBranch : AuditableEntity
{
    public Guid BankId { get; set; }
    public Bank Bank { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
