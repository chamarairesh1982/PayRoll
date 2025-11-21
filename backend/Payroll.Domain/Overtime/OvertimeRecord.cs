using Payroll.Domain.Common;

namespace Payroll.Domain.Overtime;

public class OvertimeRecord : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public double Hours { get; set; }

    public OvertimeType Type { get; set; }

    public OvertimeStatus Status { get; set; }

    public string? Reason { get; set; }

    public Guid? ApprovedById { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public bool IsLockedForPayroll { get; set; }
}
