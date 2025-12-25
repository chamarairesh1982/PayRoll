using Payroll.Domain.Common;

namespace Payroll.Domain.Employees;

/// <summary>
/// Represents an employee within a tenant organization.
/// </summary>
public class Employee : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Unique employee code within the tenant.
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// Employee's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Employee's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date when the employee joined the organization.
    /// </summary>
    public DateOnly JoinDate { get; set; }

    /// <summary>
    /// Employee's monthly base salary in LKR.
    /// </summary>
    public decimal BaseSalary { get; set; }

    /// <summary>
    /// Employee's bank account number for salary payments.
    /// </summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>
    /// Employee's EPF (Employees' Provident Fund) number.
    /// </summary>
    public string? EpfNumber { get; set; }

    /// <summary>
    /// Full name of the employee.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
