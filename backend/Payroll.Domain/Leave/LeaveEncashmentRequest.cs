using Payroll.Domain.Common;

namespace Payroll.Domain.Leave;

public class LeaveEncashmentRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public LeaveTypeCode LeaveType { get; set; }
    public decimal Days { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public LeaveEncashmentStatus Status { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? Notes { get; set; }
}
