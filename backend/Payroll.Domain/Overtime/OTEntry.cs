using Payroll.Domain.Common;

namespace Payroll.Domain.Overtime;

public class OTEntry : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public int RawMinutes { get; set; }

    public OvertimeType Type { get; set; }

    public OvertimeStatus Status { get; set; }

    public string? Comment { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? ApprovedByUserId { get; set; }

    public DateTimeOffset? ApprovedAtUtc { get; set; }

    public Guid? PayRunId { get; set; }

    public bool IsLockedForPayroll { get; set; }
}
