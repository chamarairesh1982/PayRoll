using Payroll.Domain.Common;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Domain.Employees;

public class EmployeeTaxProfile : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public bool IsTaxExempt { get; set; }
    public Guid? SlabSetOverrideId { get; set; }
    public TaxRuleSet? SlabSetOverride { get; set; }
}
