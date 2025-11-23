namespace Payroll.Domain.Loans;

public class LoanRepayment
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }

    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }

    public Loan Loan { get; set; } = null!;
}
