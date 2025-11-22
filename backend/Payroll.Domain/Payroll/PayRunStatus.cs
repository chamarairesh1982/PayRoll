namespace Payroll.Domain.Payroll;

public enum PayRunStatus
{
    Draft = 0,
    Calculated = 1,
    UnderReview = 2,
    Approved = 3,
    Posted = 4,
    Cancelled = 5
}
