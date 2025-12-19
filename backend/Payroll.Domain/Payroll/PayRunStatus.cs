namespace Payroll.Domain.Payroll;

public enum PayRunStatus
{
    Draft = 0,
    Prepared = 1,
    Approved = 2,
    Locked = 3
}

public static class PayRunStatusExtensions
{
    public static bool CanTransitionTo(this PayRunStatus currentStatus, PayRunStatus targetStatus)
    {
        return currentStatus switch
        {
            PayRunStatus.Draft => targetStatus == PayRunStatus.Prepared,
            PayRunStatus.Prepared => targetStatus == PayRunStatus.Approved,
            PayRunStatus.Approved => targetStatus == PayRunStatus.Locked,
            PayRunStatus.Locked => false,
            _ => false
        };
    }
}
