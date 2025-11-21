using Payroll.Domain.Leave;

namespace Payroll.Application.Leave.DTOs;

public class CreateLeaveRequestRequest
{
    public Guid EmployeeId { get; set; }
    public LeaveTypeCode LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool? IsHalfDay { get; set; }
    public string? HalfDaySession { get; set; }
    public string? Reason { get; set; }
}
