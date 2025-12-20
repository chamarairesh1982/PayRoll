using Payroll.Domain.Employees;

namespace Payroll.Application.DTOs.Employees;

public class CreateEmployeeRequestDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Initials { get; set; }
    public string? CallingName { get; set; }
    public string NicNumber { get; set; } = string.Empty;
    public string? EpfNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public DateTime EmploymentStartDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? BranchCode { get; set; }
    public string? BankAccountNumber { get; set; }
}
