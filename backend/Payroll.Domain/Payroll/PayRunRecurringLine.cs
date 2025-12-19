using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PayRunRecurringLine : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid RuleId { get; set; }
    public Guid? PaySlipLineId { get; set; }
    public PaySlipLineType LineType { get; set; }
}

public enum PaySlipLineType
{
    Earning = 1,
    Deduction = 2
}
