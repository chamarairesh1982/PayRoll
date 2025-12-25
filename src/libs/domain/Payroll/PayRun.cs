using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

/// <summary>
/// Represents a payroll run for a specific period within a tenant.
/// Aggregate root for payroll processing.
/// </summary>
public class PayRun : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Name/description of the pay run (e.g., "January 2025 Payroll").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Year and month of the pay period (e.g., 2025-01).
    /// </summary>
    public DateOnly PeriodMonth { get; set; }

    /// <summary>
    /// Start date of the pay period.
    /// </summary>
    public DateOnly PeriodStart { get; set; }

    /// <summary>
    /// End date of the pay period.
    /// </summary>
    public DateOnly PeriodEnd { get; set; }

    /// <summary>
    /// Date when salaries will be paid.
    /// </summary>
    public DateOnly PaymentDate { get; set; }

    /// <summary>
    /// Current status of the pay run.
    /// </summary>
    public PayRunStatus Status { get; set; } = PayRunStatus.Draft;

    /// <summary>
    /// Total gross amount for all employees in this pay run.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Total deductions for all employees in this pay run.
    /// </summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>
    /// Total net amount to be paid to all employees.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Line items for each employee in this pay run.
    /// </summary>
    public ICollection<PayRunLineItem> LineItems { get; set; } = new List<PayRunLineItem>();

    /// <summary>
    /// Calculate totals from line items.
    /// </summary>
    public void CalculateTotals()
    {
        TotalGross = LineItems.Sum(x => x.GrossAmount);
        TotalDeductions = LineItems.Sum(x => x.TotalDeductions);
        TotalNet = LineItems.Sum(x => x.NetAmount);
    }

    /// <summary>
    /// Mark the pay run as processed.
    /// </summary>
    public void Process()
    {
        if (Status != PayRunStatus.Draft)
            throw new InvalidOperationException("Only draft pay runs can be processed.");

        Status = PayRunStatus.Processed;
    }
}

/// <summary>
/// Status of a pay run.
/// </summary>
public enum PayRunStatus
{
    Draft = 0,
    Processed = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4
}
