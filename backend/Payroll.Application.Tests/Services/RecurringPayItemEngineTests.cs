using FluentAssertions;
using Payroll.Application.DTOs;
using Payroll.Application.DTOs.RecurringPayItems;
using Payroll.Application.PayrollConfig;
using Payroll.Application.RecurringPayItems;
using Payroll.Application.Services;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Domain.Payroll;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class RecurringPayItemEngineTests
{
    private static RecurringPayItemService CreateRecurringPayItemService(TestContext context)
    {
        return new RecurringPayItemService(context.DbContext, context.CurrentUserService);
    }

    private static PayrollService CreatePayrollService(TestContext context)
    {
        var epfService = new EpfEtfRuleSetService(context.DbContext, context.CurrentUserService);
        var taxService = new TaxRuleSetService(context.DbContext, context.CurrentUserService);
        var auditLogger = new AuditLogger(context.DbContext, context.CurrentUserService);
        return new PayrollService(context.DbContext, epfService, taxService, auditLogger);
    }

    [Fact]
    public void AssignmentOverlap_Should_BeRejected()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_RECUR_1", "Jane", 80_000m);
        var allowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_RECUR", "Recurring Allowance", true, true, true);
        var rule = TestDataSeeder.SeedRecurringPayItemRule(
            context.DbContext,
            "Recurring Allowance Rule",
            RecurringRuleType.Allowance,
            allowance.Id,
            5_000m,
            new DateOnly(2025, 4, 1),
            null,
            true,
            true,
            false);

        TestDataSeeder.SeedRecurringPayItemAssignment(
            context.DbContext,
            rule.Id,
            employee.Id,
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 4, 30));

        context.DbContext.RecurringPayItemAssignments.Add(new RecurringPayItemAssignment
        {
            RuleId = rule.Id,
            EmployeeId = employee.Id,
            StartDate = new DateOnly(2025, 4, 15),
            EndDate = new DateOnly(2025, 5, 15),
            IsActive = true,
            CreatedBy = "test"
        });

        Action action = () => context.DbContext.SaveChanges();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Overlapping recurring pay item assignments*");
    }

    [Fact]
    public void RuleEffectiveDates_Should_BeValidated()
    {
        using var context = new TestContext();
        var allowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_RECUR_DATES", "Recurring Allowance Dates", true, true, true);

        context.DbContext.RecurringPayItemRules.Add(new RecurringPayItemRule
        {
            Name = "Invalid Rule",
            RuleType = RecurringRuleType.Allowance,
            AllowanceTypeId = allowance.Id,
            Amount = 1_000m,
            Frequency = PayPeriodType.Monthly,
            StartDate = new DateOnly(2025, 4, 30),
            EndDate = new DateOnly(2025, 4, 1),
            Taxable = true,
            EpfEtfContributable = true,
            Prorate = false,
            CreatedBy = "test"
        });

        Action action = () => context.DbContext.SaveChanges();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*end date cannot be earlier than start date*");
    }

    [Fact]
    public async Task Simulation_Should_Proration_Correctly()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_RECUR_2", "Sam", 75_000m);
        var allowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_SIM", "Simulation Allowance", true, true, true);
        var rule = TestDataSeeder.SeedRecurringPayItemRule(
            context.DbContext,
            "Simulation Rule",
            RecurringRuleType.Allowance,
            allowance.Id,
            3_000m,
            new DateOnly(2025, 4, 1),
            null,
            true,
            true,
            true);

        TestDataSeeder.SeedRecurringPayItemAssignment(
            context.DbContext,
            rule.Id,
            employee.Id,
            new DateOnly(2025, 4, 16),
            null);

        var service = CreateRecurringPayItemService(context);
        var result = await service.SimulateAsync(new RecurringPayItemSimulationRequest
        {
            EmployeeId = employee.Id,
            PeriodStart = new DateOnly(2025, 4, 1),
            PeriodEnd = new DateOnly(2025, 4, 30)
        });

        result.Items.Should().ContainSingle();
        result.TotalAllowances.Should().Be(1_500m);
        result.TotalDeductions.Should().Be(0m);
    }

    [Fact]
    public async Task Simulation_Should_Use_PayPeriod_When_Provided()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_RECUR_4", "Ria", 60_000m);
        var allowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_SIM_PP", "Simulation Allowance PP", true, true, true);
        var rule = TestDataSeeder.SeedRecurringPayItemRule(
            context.DbContext,
            "Simulation Period Rule",
            RecurringRuleType.Allowance,
            allowance.Id,
            2_400m,
            new DateOnly(2025, 4, 1),
            null,
            true,
            true,
            false);

        TestDataSeeder.SeedRecurringPayItemAssignment(
            context.DbContext,
            rule.Id,
            employee.Id,
            new DateOnly(2025, 4, 1),
            null);

        var payRun = new PayRun
        {
            Id = Guid.NewGuid(),
            Name = "Period April 2025",
            Code = "PR_APR_2025",
            Reference = "PR_APR_2025_REF",
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            CreatedBy = "test"
        };

        context.DbContext.PayRuns.Add(payRun);
        context.DbContext.SaveChanges();

        var service = CreateRecurringPayItemService(context);
        var result = await service.SimulateAsync(new RecurringPayItemSimulationRequest
        {
            EmployeeId = employee.Id,
            PayPeriodId = payRun.Id,
            PeriodStart = default,
            PeriodEnd = default
        });

        result.Items.Should().ContainSingle();
        result.TotalAllowances.Should().Be(2_400m);
        result.TotalDeductions.Should().Be(0m);
    }

    [Fact]
    public async Task Recalculate_Should_Not_Duplicate_Recurring_Lines()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_RECUR_3", "Lee", 120_000m);
        var allowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_RECUR_ID", "Recurring Idempotency", true, true, true);

        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var rule = TestDataSeeder.SeedRecurringPayItemRule(
            context.DbContext,
            "Idempotent Rule",
            RecurringRuleType.Allowance,
            allowance.Id,
            2_000m,
            new DateOnly(2025, 4, 1),
            null,
            true,
            true,
            false);

        TestDataSeeder.SeedRecurringPayItemAssignment(
            context.DbContext,
            rule.Id,
            employee.Id,
            new DateOnly(2025, 4, 1),
            null);

        var payrollService = CreatePayrollService(context);
        var createRequest = new CreatePayRunRequest
        {
            Name = "Recurring PayRun",
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            EmployeeIds = new List<Guid> { employee.Id },
            IncludeActiveEmployeesOnly = true
        };

        var created = await payrollService.CreatePayRunAsync(createRequest);
        var lineCount = context.DbContext.PayRunRecurringLines.Count(r => r.PayRunId == created.Id);
        lineCount.Should().Be(1);

        await payrollService.RecalculatePayRunAsync(created.Id, new RecalculatePayRunRequest());

        var recalculatedLines = context.DbContext.PayRunRecurringLines.Count(r => r.PayRunId == created.Id);
        recalculatedLines.Should().Be(1);

        var updatedPayRun = await payrollService.GetPayRunAsync(created.Id);
        var paySlip = updatedPayRun!.PaySlips.Single();
        paySlip.Earnings.Count(e => e.Code == allowance.Code).Should().Be(1);
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService();
            TestDataSeeder.SeedAllowanceAndDeductionTypes(DbContext);
        }

        public Payroll.Infrastructure.Persistence.PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
