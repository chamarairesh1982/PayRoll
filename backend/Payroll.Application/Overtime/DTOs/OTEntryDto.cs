using Payroll.Domain.Overtime;

namespace Payroll.Application.Overtime.DTOs;

public class OTEntryDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }

    public DateTime WorkDate { get; set; }

    public int RawMinutes { get; set; }

    public OvertimeType Type { get; set; }

    public OvertimeStatus Status { get; set; }

    public string? Comment { get; set; }

    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Guid? PayRunId { get; set; }

    public bool IsLockedForPayroll { get; set; }
}
