using FluentAssertions;
using Payroll.Application.DTOs;
using Payroll.Application.PayrollConfig;
using Payroll.Application.Services;
using Payroll.Domain.Payroll;
using Payroll.Domain.Overtime;
using Payroll.Application.Tests.TestInfrastructure;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class PayrollServiceTests
{
    private static PayrollService CreatePayrollService(TestContext context)
    {
        var epfService = new EpfEtfRuleSetService(context.DbContext, context.CurrentUserService);
        var taxService = new TaxRuleSetService(context.DbContext, context.CurrentUserService);
        return new PayrollService(context.DbContext, epfService, taxService);
    }

    [Fact]
    public async Task CreatePayRun_Should_Calculate_EpfEtf_For_BasicOnly_And_NoTax_WhenBelowThreshold()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP001", "Alice", 90_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        paySlip.TotalEarnings.Should().Be(90_000m);
        paySlip.Deductions.Should().ContainSingle(d => d.Code == "EPF_EE" && d.Amount == 7_200m);
        paySlip.PayeTax.Should().Be(0m);
        paySlip.EmployeeEpf.Should().Be(7_200m);
        paySlip.EmployerEpf.Should().Be(10_800m);
        paySlip.EmployerEtf.Should().Be(2_700m);
        paySlip.NetPay.Should().Be(82_800m);

        result.TotalNetPay.Should().Be(82_800m);
        result.EmployeeCount.Should().Be(1);
    }

    [Fact]
    public async Task CreatePayRun_Should_Include_Overtime_In_Earnings_And_EpfEtf()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP002", "Bob", 100_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        TestDataSeeder.SeedOvertime(context.DbContext, employee, new DateOnly(2025, 4, 15), 10, OvertimeType.Weekday);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var baseHourlyRate = 100_000m / (26 * 8);
        var expectedOt = Math.Round(baseHourlyRate * 10 * 1.5m, 2, MidpointRounding.AwayFromZero);
        var expectedEpfBase = 100_000m + expectedOt;
        var expectedEmployeeEpf = Math.Round(expectedEpfBase * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedEmployerEpf = Math.Round(expectedEpfBase * 0.12m, 2, MidpointRounding.AwayFromZero);
        var expectedEmployerEtf = Math.Round(expectedEpfBase * 0.03m, 2, MidpointRounding.AwayFromZero);

        paySlip.Earnings.Should().Contain(e => e.Code == "OT" && e.Amount == expectedOt);
        paySlip.TotalEarnings.Should().Be(100_000m + expectedOt);
        paySlip.EmployeeEpf.Should().Be(expectedEmployeeEpf);
        paySlip.EmployerEpf.Should().Be(expectedEmployerEpf);
        paySlip.EmployerEtf.Should().Be(expectedEmployerEtf);
    }

    [Fact]
    public async Task CreatePayRun_Should_Apply_NoPay_Deduction_For_Absences()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP003", "Carol", 100_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        TestDataSeeder.SeedAbsence(context.DbContext, employee, new DateOnly(2025, 4, 5));
        TestDataSeeder.SeedAbsence(context.DbContext, employee, new DateOnly(2025, 4, 10));

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var dailyRate = Math.Round(100_000m / 26m, 2, MidpointRounding.AwayFromZero);
        var expectedNoPay = Math.Round(dailyRate * 2, 2, MidpointRounding.AwayFromZero);
        var expectedEpf = Math.Round(100_000m * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedTotalDeductions = expectedEpf + expectedNoPay;
        var expectedNet = Math.Round(100_000m - expectedTotalDeductions, 2, MidpointRounding.AwayFromZero);

        paySlip.Deductions.Should().Contain(d => d.Code == "NOPAY" && d.Amount == expectedNoPay);
        paySlip.EmployeeEpf.Should().Be(expectedEpf);
        paySlip.TotalDeductions.Should().Be(expectedTotalDeductions);
        paySlip.NetPay.Should().Be(expectedNet);
    }

    [Fact]
    public async Task CreatePayRun_Should_Deduct_LoanInstallment_And_Reduce_OutstandingPrincipal()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP004", "Dan", 150_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        var loan = TestDataSeeder.SeedActiveLoan(context.DbContext, employee, 120_000m, 10_000m);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        paySlip.Deductions.Should().Contain(d => d.Code == "LOAN" && d.Amount == 10_000m);
        loan.OutstandingPrincipal.Should().Be(110_000m);
        paySlip.NetPay.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task CreatePayRun_Should_Apply_PayeTax_Using_TaxSlabs()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP005", "Eve", 200_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var taxableIncome = 200_000m - Math.Round(200_000m * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedPaye = CalculateProgressiveTax(taxableIncome);

        paySlip.Deductions.Should().Contain(d => d.Code == "PAYE" && d.Amount == expectedPaye);
        paySlip.PayeTax.Should().Be(expectedPaye);
        paySlip.NetPay.Should().Be(200_000m - paySlip.TotalDeductions);
    }

    private static CreatePayRunRequest BuildDefaultRequest(Guid employeeId)
    {
        return new CreatePayRunRequest
        {
            Name = "April 2025",
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            EmployeeIds = new List<Guid> { employeeId },
            IncludeActiveEmployeesOnly = false
        };
    }

    private static decimal CalculateProgressiveTax(decimal taxableIncome)
    {
        var slabs = new[]
        {
            (From: 0m, To: 100_000m, Rate: 0m),
            (From: 100_000m, To: 141_667m, Rate: 6m),
            (From: 141_667m, To: 183_333m, Rate: 12m),
            (From: 183_333m, To: (decimal?)null, Rate: 18m)
        };

        decimal total = 0;
        foreach (var slab in slabs)
        {
            if (taxableIncome <= slab.From)
            {
                continue;
            }

            var upper = slab.To ?? decimal.MaxValue;
            var chargeable = Math.Min(taxableIncome, upper) - slab.From;
            total += chargeable * slab.Rate / 100m;
        }

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
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
