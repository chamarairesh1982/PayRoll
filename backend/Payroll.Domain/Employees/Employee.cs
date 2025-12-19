using Payroll.Domain.Common;
using Payroll.Domain.Organizations;
using System.Linq;

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
        string? epfNumber,
        DateTime dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        DateTime employmentStartDate,
        decimal baseSalary,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        string? initials,
        string? callingName,
        DateTime? probationEndDate,
        DateTime? confirmationDate,
        string createdBy,
        string? bankName,
        string? bankCode,
        string? branchCode,
        string? bankAccountNumber)
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
        CompanyId = companyId;
        BranchId = branchId;
        CostCenterId = costCenterId;
        ProbationEndDate = probationEndDate;
        ConfirmationDate = confirmationDate;
        EpfNumber = NormalizeBankField(epfNumber);
        CreatedBy = createdBy;
        BankName = NormalizeBankField(bankName);
        BankCode = NormalizeBankField(bankCode);
        BranchCode = NormalizeBankField(branchCode);
        BankAccountNumber = NormalizeBankField(bankAccountNumber);
    }

    public string EmployeeCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Initials { get; private set; }
    public string? CallingName { get; private set; }
    public string NicNumber { get; private set; } = string.Empty;
    public string? EpfNumber { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public DateTime EmploymentStartDate { get; private set; }
    public DateTime? ProbationEndDate { get; private set; }
    public DateTime? ConfirmationDate { get; private set; }
    public decimal BaseSalary { get; private set; }
    public string? BankName { get; private set; }
    public string? BankCode { get; private set; }
    public string? BranchCode { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid? BranchId { get; private set; }
    public Branch? Branch { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public CostCenter? CostCenter { get; private set; }
    public string Code => EmployeeCode;
    public string FullName => string.Join(" ", new[] { FirstName, LastName }.Where(n => !string.IsNullOrWhiteSpace(n)));

    public static Employee Create(
        string employeeCode,
        string firstName,
        string lastName,
        string nicNumber,
        string? epfNumber,
        DateTime dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        DateTime employmentStartDate,
        decimal baseSalary,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        string? initials,
        string? callingName,
        DateTime? probationEndDate,
        DateTime? confirmationDate,
        string createdBy,
        string? bankName = null,
        string? bankCode = null,
        string? branchCode = null,
        string? bankAccountNumber = null)
    {
        return new Employee(
            employeeCode,
            firstName,
            lastName,
            nicNumber,
            epfNumber,
            dateOfBirth,
            gender,
            maritalStatus,
            employmentStartDate,
            baseSalary,
            companyId,
            branchId,
            costCenterId,
            initials,
            callingName,
            probationEndDate,
            confirmationDate,
            createdBy,
            bankName,
            bankCode,
            branchCode,
            bankAccountNumber);
    }

    public void Update(
        string employeeCode,
        string firstName,
        string lastName,
        string nicNumber,
        string? epfNumber,
        DateTime dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        DateTime employmentStartDate,
        decimal baseSalary,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        string? initials,
        string? callingName,
        DateTime? probationEndDate,
        DateTime? confirmationDate,
        string modifiedBy,
        string? bankName = null,
        string? bankCode = null,
        string? branchCode = null,
        string? bankAccountNumber = null)
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
        CompanyId = companyId;
        BranchId = branchId;
        CostCenterId = costCenterId;
        ProbationEndDate = probationEndDate;
        ConfirmationDate = confirmationDate;
        EpfNumber = NormalizeBankField(epfNumber);
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdateBankDetails(bankName, bankCode, branchCode, bankAccountNumber);
    }

    public void UpdateBankDetails(string? bankName, string? bankCode, string? branchCode, string? bankAccountNumber)
    {
        BankName = NormalizeBankField(bankName);
        BankCode = NormalizeBankField(bankCode);
        BranchCode = NormalizeBankField(branchCode);
        BankAccountNumber = NormalizeBankField(bankAccountNumber);
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

    private static string? NormalizeBankField(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
