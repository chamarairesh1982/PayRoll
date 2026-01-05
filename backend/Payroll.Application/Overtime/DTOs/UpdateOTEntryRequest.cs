using Payroll.Domain.Overtime;

namespace Payroll.Application.Overtime.DTOs;

public class UpdateOTEntryRequest
{
    public DateTime? WorkDate { get; set; }
    public int? RawMinutes { get; set; }
    public OvertimeType? Type { get; set; }
    public string? Comment { get; set; }
}
