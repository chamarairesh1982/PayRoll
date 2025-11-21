using Payroll.Domain.Payroll;

namespace Payroll.Application.Payroll.Requests;

public class CreatePayRunRequest
{
    public string Name { get; set; } = null!;
    public PayPeriodType PeriodType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }

    public bool IncludeActiveEmployeesOnly { get; set; } = true;
    public List<Guid>? EmployeeIds { get; set; }
}
