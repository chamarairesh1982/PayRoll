namespace Payroll.Domain.Payroll;

public enum PayRunStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Posted = 3,
    Archived = 4
}
