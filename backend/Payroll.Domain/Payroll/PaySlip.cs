using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class PaySlip : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public decimal NetPay { get; set; }
    public ICollection<EarningLine> Earnings { get; set; } = new List<EarningLine>();
    public ICollection<DeductionLine> Deductions { get; set; } = new List<DeductionLine>();
    public ICollection<StatutoryContribution> StatutoryContributions { get; set; } = new List<StatutoryContribution>();
}
