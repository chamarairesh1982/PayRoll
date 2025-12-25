using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

/// <summary>
/// Represents a single employee's payroll calculation within a pay run.
/// </summary>
public class PayRunLineItem : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Reference to the parent pay run.
    /// </summary>
    public Guid PayRunId { get; set; }
    public PayRun PayRun { get; set; } = null!;

    /// <summary>
    /// Reference to the employee.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Employee code (denormalized for reporting).
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// Employee name (denormalized for reporting).
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Base salary for this period.
    /// </summary>
    public decimal BaseSalary { get; set; }

    /// <summary>
    /// Allowances (e.g., transport, meals).
    /// </summary>
    public decimal Allowances { get; set; }

    /// <summary>
    /// Overtime amount.
    /// </summary>
    public decimal Overtime { get; set; }

    /// <summary>
    /// Gross amount (Base + Allowances + Overtime).
    /// </summary>
    public decimal GrossAmount { get; set; }

    /// <summary>
    /// EPF employee contribution (8% of gross).
    /// </summary>
    public decimal EpfEmployee { get; set; }

    /// <summary>
    /// EPF employer contribution (12% of gross).
    /// </summary>
    public decimal EpfEmployer { get; set; }

    /// <summary>
    /// ETF employer contribution (3% of gross).
    /// </summary>
    public decimal Etf { get; set; }

    /// <summary>
    /// PAYE tax amount.
    /// </summary>
    public decimal Tax { get; set; }

    /// <summary>
    /// Other deductions (loans, advances, etc.).
    /// </summary>
    public decimal OtherDeductions { get; set; }

    /// <summary>
    /// Total deductions.
    /// </summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>
    /// Net amount to be paid (Gross - TotalDeductions).
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Calculate all amounts based on base salary and inputs.
    /// </summary>
    public void Calculate()
    {
        // Calculate gross
        GrossAmount = BaseSalary + Allowances + Overtime;

        // Calculate statutory deductions (Sri Lanka rates)
        EpfEmployee = GrossAmount * 0.08m; // 8%
        EpfEmployer = GrossAmount * 0.12m; // 12%
        Etf = GrossAmount * 0.03m;         // 3%

        // TODO: Tax calculation based on tax slabs (simplified for now)
        Tax = CalculateTax(GrossAmount);

        // Calculate total deductions
        TotalDeductions = EpfEmployee + Tax + OtherDeductions;

        // Calculate net
        NetAmount = GrossAmount - TotalDeductions;
    }

    private decimal CalculateTax(decimal grossAmount)
    {
        // Simplified tax calculation
        // TODO: Implement proper Sri Lankan tax slab calculation
        if (grossAmount <= 100000m)
            return 0m;
        
        if (grossAmount <= 141667m)
            return (grossAmount - 100000m) * 0.06m;
        
        return ((grossAmount - 141667m) * 0.12m) + (41667m * 0.06m);
    }
}
