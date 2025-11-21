using Payroll.Domain.Common;

namespace Payroll.Domain.Employees;

public class Employee : AuditableEntity, IAggregateRoot
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Probation;
    public EmployeeType EmployeeType { get; set; } = EmployeeType.Permanent;
    public decimal BasicSalary { get; set; }
}
