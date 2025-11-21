using Payroll.Domain.Leave;

namespace Payroll.Application.Leave.DTOs;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }

    public LeaveTypeCode LeaveType { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double TotalDays { get; set; }

    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; }

    public Guid? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public bool? IsHalfDay { get; set; }
    public string? HalfDaySession { get; set; }
}
