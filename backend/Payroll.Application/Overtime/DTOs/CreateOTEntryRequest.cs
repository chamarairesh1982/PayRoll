using Payroll.Domain.Overtime;

namespace Payroll.Application.Overtime.DTOs;

public class CreateOTEntryRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public int RawMinutes { get; set; }
    public OvertimeType Type { get; set; }
    public OvertimeStatus Status { get; set; } = OvertimeStatus.Draft;
    public string? Comment { get; set; }
}
