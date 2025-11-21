using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class PayRun : AuditableEntity, IAggregateRoot
{
    public string Reference { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public PayRunStatus Status { get; set; } = PayRunStatus.Draft;
    public ICollection<PaySlip> PaySlips { get; set; } = new List<PaySlip>();
}
