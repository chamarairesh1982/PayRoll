using FluentAssertions;
using Payroll.Application.Security;
using Payroll.Application.Services;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Domain.Employees;
using Payroll.Infrastructure.Persistence;
using Payroll.Application.Interfaces;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class EmployeeServiceMaskingTests
{
    [Fact]
    public async Task GetByIdAsync_Should_Mask_Sensitive_Data_For_Non_Admins()
    {
        using var context = new TestContext();
        var employee = SeedEmployee(context.DbContext, "EMP-MASK-1", "Nico", "901234567V", "1234567890");

        var service = new EmployeeService(context.DbContext, context.CurrentUserService, new NoOpAuditLogger());

        var result = await service.GetByIdAsync(employee.Id);

        result.Should().NotBeNull();
        result!.NicNumber.Should().BeEmpty();
        result.MaskedNicNumber.Should().Be(SensitiveDataMasker.MaskNic("901234567V"));
        result.BankAccountNumber.Should().BeNull();
        result.MaskedBankAccountNumber.Should().Be(SensitiveDataMasker.MaskBankAccount("1234567890"));
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Full_Sensitive_Data_For_Admins()
    {
        using var context = new TestContext(new[] { "Admin" });
        var employee = SeedEmployee(context.DbContext, "EMP-MASK-2", "Mila", "991234567V", "9876543210");

        var service = new EmployeeService(context.DbContext, context.CurrentUserService, new NoOpAuditLogger());

        var result = await service.GetByIdAsync(employee.Id);

        result.Should().NotBeNull();
        result!.NicNumber.Should().Be("991234567V");
        result.MaskedNicNumber.Should().Be(SensitiveDataMasker.MaskNic("991234567V"));
        result.BankAccountNumber.Should().Be("9876543210");
        result.MaskedBankAccountNumber.Should().Be(SensitiveDataMasker.MaskBankAccount("9876543210"));
    }

    private static Employee SeedEmployee(
        PayrollDbContext context,
        string code,
        string name,
        string nicNumber,
        string bankAccountNumber)
    {
        var employee = Employee.Create(
            employeeCode: code,
            firstName: name,
            lastName: "Test",
            nicNumber: nicNumber,
            epfNumber: null,
            dateOfBirth: new DateTime(1990, 1, 1),
            gender: Gender.Male,
            maritalStatus: MaritalStatus.Single,
            employmentStartDate: new DateTime(2020, 1, 1),
            baseSalary: 100_000m,
            hourlyRate: null,
            companyId: null,
            branchId: null,
            costCenterId: null,
            initials: null,
            callingName: null,
            probationEndDate: new DateTime(2020, 4, 1),
            confirmationDate: null,
            createdBy: "seed",
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: bankAccountNumber);

        context.Employees.Add(employee);
        context.SaveChanges();
        return employee;
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext(string[]? roles = null)
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService(roles ?? Array.Empty<string>());
        }

        public PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public TestCurrentUserService(IReadOnlyCollection<string> roles)
        {
            Roles = roles;
        }

        public string? UserId => "test-user";
        public string? UserName => "test-user";
        public IReadOnlyCollection<string> Roles { get; }
    }

    private sealed class NoOpAuditLogger : IAuditLogger
    {
        public Task LogAsync(
            string entityName,
            string entityId,
            string action,
            object? beforeSnapshot,
            object? afterSnapshot,
            string? performedBy = null,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
