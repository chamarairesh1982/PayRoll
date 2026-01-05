using Payroll.Domain.Common;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Domain.Payroll;

public class RecurringPayItemRule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public RecurringRuleType RuleType { get; set; }
    public Guid? AllowanceTypeId { get; set; }
    public Guid? DeductionTypeId { get; set; }
    public AllowanceType? AllowanceType { get; set; }
    public DeductionType? DeductionType { get; set; }
    public decimal Amount { get; set; }
    public PayPeriodType Frequency { get; set; } = PayPeriodType.Monthly;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool Taxable { get; set; }
    public bool EpfEtfContributable { get; set; }
    public bool Prorate { get; set; }
    public ICollection<RecurringPayItemAssignment> Assignments { get; set; } = new List<RecurringPayItemAssignment>();
}
