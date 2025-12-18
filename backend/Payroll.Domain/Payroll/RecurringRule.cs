using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class RecurringRule : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public RecurringRuleType RuleType { get; set; }
    public PayPeriodType Frequency { get; set; } = PayPeriodType.Monthly;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal Amount { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public bool IsEpfApplicable { get; set; }
    public bool IsEtfApplicable { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum RecurringRuleType
{
    Allowance = 1,
    Deduction = 2
}
