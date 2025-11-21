using Payroll.Domain.Common;

namespace Payroll.Domain.Leave;

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public LeaveTypeCode LeaveType { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public double TotalDays { get; set; }

    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; }

    public Guid? ApprovedById { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public bool? IsHalfDay { get; set; }
    public string? HalfDaySession { get; set; }
}
