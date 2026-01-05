using Payroll.Domain.Common;

namespace Payroll.Domain.Loans;

public class Loan : AuditableEntity, IAggregateRoot
{
    public Guid EmployeeId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal InstallmentAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Draft;
    public ICollection<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();
}
