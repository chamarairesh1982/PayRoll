using Payroll.Domain.Overtime;

namespace Payroll.Application.Overtime.DTOs;

public class OvertimeRecordDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }

    public DateTime Date { get; set; }

    public double Hours { get; set; }

    public OvertimeType Type { get; set; }

    public OvertimeStatus Status { get; set; }

    public string? Reason { get; set; }

    public Guid? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public bool IsLockedForPayroll { get; set; }
}
