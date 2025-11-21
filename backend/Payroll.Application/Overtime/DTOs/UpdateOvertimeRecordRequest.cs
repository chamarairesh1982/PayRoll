using Payroll.Domain.Overtime;

namespace Payroll.Application.Overtime.DTOs;

public class UpdateOvertimeRecordRequest
{
    public DateTime? Date { get; set; }
    public double? Hours { get; set; }
    public OvertimeType? Type { get; set; }
    public string? Reason { get; set; }
    public OvertimeStatus? Status { get; set; }
}
