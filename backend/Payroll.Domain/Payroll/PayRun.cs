using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PayRun : AuditableEntity, IAggregateRoot
{
    public new Guid Id { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public PayPeriodType PeriodType { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    public DateOnly PayDate { get; set; }

    public PayRunStatus Status { get; set; }

    public bool IsLocked { get; set; }

    public ICollection<PaySlip> PaySlips { get; set; } = new List<PaySlip>();
}
