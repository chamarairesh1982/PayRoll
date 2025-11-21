namespace Payroll.Domain.Payroll;

public class DeductionLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
