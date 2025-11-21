namespace Payroll.Domain.Loans;

public class LoanRepayment
{
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}
