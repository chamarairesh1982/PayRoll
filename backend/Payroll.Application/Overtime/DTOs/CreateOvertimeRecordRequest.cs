using Payroll.Domain.Overtime;

namespace Payroll.Application.Overtime.DTOs;

public class CreateOvertimeRecordRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public double Hours { get; set; }
    public OvertimeType Type { get; set; }
    public string? Reason { get; set; }
}
