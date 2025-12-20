namespace Payroll.Domain.GeneralLedger;

public enum GlJournalBatchStatus
{
    Draft = 1,
    Generated = 2,
    Approved = 3,
    Exported = 4,
    Failed = 5
}
