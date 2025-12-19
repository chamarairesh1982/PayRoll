using FluentAssertions;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig;
using Payroll.Application.Services;
using Payroll.Application.StatutoryReports;
using Payroll.Application.Tests.Services;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Xunit;

namespace Payroll.Application.Tests.StatutoryReports;

public class StatutoryReportServiceTests
{
    [Fact]
    public async Task GenerateEpfEtfReport_Should_Require_Locked_PayRun()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_LOCK", "Liam", 80_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRun = await payrollService.CreatePayRunAsync(BuildDefaultRequest(employee.Id));

        var service = new StatutoryReportService(context.DbContext, context.CurrentUserService, context.Storage);

        var action = () => service.GenerateEpfEtfReportAsync(new EpfEtfReportRequestDto
        {
            PayRunId = payRun.Id,
            Format = "csv"
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("EPF/ETF reports can only be generated for locked pay runs.");
    }

    [Fact]
    public async Task GenerateEpfEtfReport_Should_Summarize_Contributable_Base_And_Warnings()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP001", "Ava", 100_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var allowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_NC", "Non Contrib", false, false, false);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, allowance.Code, 10_000m, null);

        var payrollService = CreatePayrollService(context);
        var payRun = await payrollService.CreatePayRunAsync(BuildDefaultRequest(employee.Id));

        var payRunEntity = await context.DbContext.PayRuns.FindAsync(payRun.Id);
        payRunEntity!.Status = PayRunStatus.Locked;
        payRunEntity.IsLocked = true;
        await context.DbContext.SaveChangesAsync();

        var service = new StatutoryReportService(context.DbContext, context.CurrentUserService, context.Storage);
        var report = await service.GenerateEpfEtfReportAsync(new EpfEtfReportRequestDto
        {
            PayRunId = payRun.Id,
            Format = "csv"
        });

        report.EmployeeCount.Should().Be(1);
        report.ContributableBase.Should().Be(100_000m);
        report.EmployeeEpfTotal.Should().Be(8_000m);
        report.EmployerEpfTotal.Should().Be(12_000m);
        report.EmployerEtfTotal.Should().Be(3_000m);
        report.Warnings.Should().Contain(w => w.Message == "Missing EPF number.");
    }

    private static PayrollService CreatePayrollService(TestContext context)
    {
        var epfService = new EpfEtfRuleSetService(context.DbContext, context.CurrentUserService);
        var taxService = new TaxRuleSetService(context.DbContext, context.CurrentUserService);
        var auditLogger = new AuditLogger(context.DbContext, context.CurrentUserService);
        return new PayrollService(context.DbContext, epfService, taxService, auditLogger, new FakeCurrentUserService());
    }

    private static CreatePayRunRequest BuildDefaultRequest(Guid employeeId)
    {
        return new CreatePayRunRequest
        {
            Name = "April Payroll",
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            EmployeeIds = new List<Guid> { employeeId },
            IncludeActiveEmployeesOnly = true
        };
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService();
            Storage = new InMemoryReportStorage();
            TestDataSeeder.SeedAllowanceAndDeductionTypes(DbContext);
        }

        public Payroll.Infrastructure.Persistence.PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }
        public InMemoryReportStorage Storage { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private sealed class InMemoryReportStorage : IStatutoryReportStorage
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
        {
            var path = $"memory://{Guid.NewGuid()}-{fileName}";
            _storage[path] = content;
            return Task.FromResult(path);
        }

        public Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage[filePath]);
        }
    }
}
