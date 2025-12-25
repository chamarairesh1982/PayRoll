using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

/// <summary>
/// Represents a payslip document for an employee.
/// Generated from a PayRunLineItem.
/// </summary>
public class Payslip : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Reference to the pay run.
    /// </summary>
    public Guid PayRunId { get; set; }

    /// <summary>
    /// Reference to the employee.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Reference to the pay run line item.
    /// </summary>
    public Guid PayRunLineItemId { get; set; }

    /// <summary>
    /// Unique payslip number.
    /// </summary>
    public string PayslipNumber { get; set; } = string.Empty;

    /// <summary>
    /// Period month (e.g., "January 2025").
    /// </summary>
    public string PeriodDescription { get; set; } = string.Empty;

    /// <summary>
    /// Path to the generated PDF file (if generated).
    /// </summary>
    public string? PdfFilePath { get; set; }

    /// <summary>
    /// Indicates whether the payslip has been sent to the employee.
    /// </summary>
    public bool IsSent { get; set; }

    /// <summary>
    /// Date when the payslip was sent.
    /// </summary>
    public DateTime? SentAt { get; set; }
}
