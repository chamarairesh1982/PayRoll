using Payroll.Domain.Employees;

namespace Payroll.Application.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public EmploymentStatus EmploymentStatus { get; set; }
    public EmployeeType EmployeeType { get; set; }
}
