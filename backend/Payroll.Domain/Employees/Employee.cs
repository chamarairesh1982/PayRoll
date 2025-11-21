using Payroll.Domain.Common;

namespace Payroll.Domain.Employees;

public class Employee : AuditableEntity, IAggregateRoot
{
    private Employee()
    {
    }

    private Employee(
        string employeeCode,
        string firstName,
        string lastName,
        string nicNumber,
        DateTime dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        DateTime employmentStartDate,
        decimal baseSalary,
        string? initials,
        string? callingName,
        DateTime? probationEndDate,
        DateTime? confirmationDate,
        string createdBy)
    {
        EmployeeCode = ValidateRequired(employeeCode, nameof(EmployeeCode));
        FirstName = ValidateRequired(firstName, nameof(FirstName));
        LastName = ValidateRequired(lastName, nameof(LastName));
        NicNumber = ValidateRequired(nicNumber, nameof(NicNumber));
        DateOfBirth = ValidateDateOfBirth(dateOfBirth);
        EmploymentStartDate = employmentStartDate;
        BaseSalary = ValidateBaseSalary(baseSalary);
        Gender = gender;
        MaritalStatus = maritalStatus;
        Initials = initials;
        CallingName = callingName;
        ProbationEndDate = probationEndDate;
        ConfirmationDate = confirmationDate;
        CreatedBy = createdBy;
    }

    public string EmployeeCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Initials { get; private set; }
    public string? CallingName { get; private set; }
    public string NicNumber { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public DateTime EmploymentStartDate { get; private set; }
    public DateTime? ProbationEndDate { get; private set; }
    public DateTime? ConfirmationDate { get; private set; }
    public decimal BaseSalary { get; private set; }

    public static Employee Create(
        string employeeCode,
        string firstName,
        string lastName,
        string nicNumber,
        DateTime dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        DateTime employmentStartDate,
        decimal baseSalary,
        string? initials,
        string? callingName,
        DateTime? probationEndDate,
        DateTime? confirmationDate,
        string createdBy)
    {
        return new Employee(
            employeeCode,
            firstName,
            lastName,
            nicNumber,
            dateOfBirth,
            gender,
            maritalStatus,
            employmentStartDate,
            baseSalary,
            initials,
            callingName,
            probationEndDate,
            confirmationDate,
            createdBy);
    }

    public void Update(
        string employeeCode,
        string firstName,
        string lastName,
        string nicNumber,
        DateTime dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        DateTime employmentStartDate,
        decimal baseSalary,
        string? initials,
        string? callingName,
        DateTime? probationEndDate,
        DateTime? confirmationDate,
        string modifiedBy)
    {
        EmployeeCode = ValidateRequired(employeeCode, nameof(EmployeeCode));
        FirstName = ValidateRequired(firstName, nameof(FirstName));
        LastName = ValidateRequired(lastName, nameof(LastName));
        NicNumber = ValidateRequired(nicNumber, nameof(NicNumber));
        DateOfBirth = ValidateDateOfBirth(dateOfBirth);
        Gender = gender;
        MaritalStatus = maritalStatus;
        EmploymentStartDate = employmentStartDate;
        BaseSalary = ValidateBaseSalary(baseSalary);
        Initials = initials;
        CallingName = callingName;
        ProbationEndDate = probationEndDate;
        ConfirmationDate = confirmationDate;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    public void SoftDelete(string modifiedBy)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Employee is already inactive.");
        }

        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    private static string ValidateRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} is required", propertyName);
        }

        return value.Trim();
    }

    private static DateTime ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth.Date >= DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Date of birth must be in the past", nameof(DateOfBirth));
        }

        return dateOfBirth.Date;
    }

    private static decimal ValidateBaseSalary(decimal baseSalary)
    {
        if (baseSalary <= 0)
        {
            throw new ArgumentException("Base salary must be greater than zero", nameof(BaseSalary));
        }

        return baseSalary;
    }
}
