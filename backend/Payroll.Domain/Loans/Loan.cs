using Payroll.Domain.Common;

namespace Payroll.Domain.Loans;

public class Loan : AuditableEntity, IAggregateRoot
{
    public Guid EmployeeId { get; set; }
    public decimal Principal { get; set; }
    public decimal Outstanding { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Draft;
    public ICollection<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();
}
