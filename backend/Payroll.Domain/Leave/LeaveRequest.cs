using Payroll.Domain.Common;
using Payroll.Domain.ValueObjects;

namespace Payroll.Domain.Leave;

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public DateRange Range { get; set; }
    public string Reason { get; set; } = string.Empty;
}
