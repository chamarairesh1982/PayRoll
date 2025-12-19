using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class RecurringPayItemAssignment : AuditableEntity
{
    public Guid RuleId { get; set; }
    public RecurringPayItemRule? Rule { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
