using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class PaySlip : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public PayRun? PayRun { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public decimal EmployeeEpf { get; set; }
    public decimal EmployerEpf { get; set; }
    public decimal EmployerEtf { get; set; }
    public decimal PayeTax { get; set; }
    public ICollection<EarningLine> Earnings { get; set; } = new List<EarningLine>();
    public ICollection<DeductionLine> Deductions { get; set; } = new List<DeductionLine>();
}
