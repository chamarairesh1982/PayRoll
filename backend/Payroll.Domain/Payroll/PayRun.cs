using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class PayRun : AuditableEntity, IAggregateRoot
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PayPeriodType PeriodType { get; set; } = PayPeriodType.Monthly;
    public string Reference { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public bool IsLocked { get; set; }
    public PayRunStatus Status { get; set; } = PayRunStatus.Draft;
    public ICollection<PaySlip> PaySlips { get; set; } = new List<PaySlip>();
}

public enum PayPeriodType
{
    Monthly = 1,
    Weekly = 2,
    Custom = 3
}
