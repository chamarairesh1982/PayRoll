using Payroll.Domain.Leave;

namespace Payroll.Application.DTOs;

public class LeaveDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public LeaveStatus LeaveStatus { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
