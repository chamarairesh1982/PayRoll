namespace Payroll.Domain.Payroll;

public enum PayRunStatus
{
    Draft = 1,
    Calculated = 2,
    UnderReview = 3,
    Approved = 4,
    Posted = 5,
    Cancelled = 6
}
