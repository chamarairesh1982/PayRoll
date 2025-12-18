using FluentAssertions;
using Payroll.Application.DTOs;
using Payroll.Application.PayrollConfig;
using Payroll.Application.Services;
using Payroll.Domain.Leave;
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
        var auditLogger = new AuditLogger(context.DbContext, context.CurrentUserService);
        return new PayrollService(context.DbContext, epfService, taxService, auditLogger);
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
    public async Task CreatePayRun_Should_Apply_Fixed_Recurring_Allowance()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP010", "Fiona", 100_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var allowanceType = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_TRAN", "Transport Allowance", true, true, true);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, allowanceType.Code, 5_000m, null);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var expectedEpfBase = 105_000m;
        var expectedEmployeeEpf = Math.Round(expectedEpfBase * 0.08m, 2, MidpointRounding.AwayFromZero);

        paySlip.Earnings.Should().Contain(e => e.Code == "ALW_TRAN" && e.Amount == 5_000m && e.IsEpfApplicable && e.IsTaxable);
        paySlip.TotalEarnings.Should().Be(105_000m);
        paySlip.EmployeeEpf.Should().Be(expectedEmployeeEpf);
        paySlip.NetPay.Should().Be(105_000m - paySlip.TotalDeductions);
    }

    [Fact]
    public async Task CreatePayRun_Should_Apply_Percentage_Allowance_On_Basic_Salary()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP011", "Grace", 100_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var allowanceType = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_COMM", "Commission", true, false, false);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, allowanceType.Code, null, 10m);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var expectedAllowance = 10_000m;
        var expectedEmployeeEpf = Math.Round(100_000m * 0.08m, 2, MidpointRounding.AwayFromZero);

        paySlip.Earnings.Should().Contain(e => e.Code == "ALW_COMM" && e.Amount == expectedAllowance && !e.IsEpfApplicable);
        paySlip.TotalEarnings.Should().Be(110_000m);
        paySlip.EmployeeEpf.Should().Be(expectedEmployeeEpf);
    }

    [Fact]
    public async Task CreatePayRun_Should_Respect_Taxable_Flag_For_Allowances()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP012", "Hank", 200_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var taxableAllowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_TAX", "Taxable Allowance", true, false, false);
        var nonTaxableAllowance = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_NONTAX", "Non Taxable Allowance", false, false, false);

        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, taxableAllowance.Code, 10_000m, null);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, nonTaxableAllowance.Code, 5_000m, null);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var expectedTaxableIncome = (200_000m + 10_000m) - Math.Round(200_000m * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedPaye = CalculateProgressiveTax(expectedTaxableIncome);

        paySlip.Earnings.Should().Contain(e => e.Code == "ALW_TAX" && e.IsTaxable);
        paySlip.Earnings.Should().Contain(e => e.Code == "ALW_NONTAX" && !e.IsTaxable);
        paySlip.TotalEarnings.Should().Be(215_000m);
        paySlip.PayeTax.Should().Be(expectedPaye);
    }

    [Fact]
    public async Task CreatePayRun_Should_Apply_Fixed_Allowance_Amount()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP020", "Harry", 80_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var allowanceType = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_MEAL", "Meal Allowance", true, true, true);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, allowanceType.Code, 7_500m, null);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        paySlip.Earnings.Should().Contain(e => e.Code == "ALW_MEAL" && e.Amount == 7_500m);
        paySlip.TotalEarnings.Should().Be(87_500m);
    }

    [Fact]
    public async Task CreatePayRun_Should_Apply_Percentage_Allowance_As_Percentage_Of_Basic()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP021", "Isla", 90_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var allowanceType = TestDataSeeder.SeedAllowanceType(context.DbContext, "ALW_PER", "Performance Bonus", true, true, true);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Allowance, allowanceType.Code, null, 15m);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        paySlip.Earnings.Should().Contain(e => e.Code == "ALW_PER" && e.Amount == 13_500m);
        paySlip.TotalEarnings.Should().Be(103_500m);
    }

    [Fact]
    public async Task CreatePayRun_Should_Reduce_TaxableIncome_With_PreTax_Deduction()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP022", "Jane", 150_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var deductionType = TestDataSeeder.SeedDeductionType(context.DbContext, "DED_GROSS", "Gross Reduction", true, false);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Deduction, deductionType.Code, 5_000m, null);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var epf = Math.Round(150_000m * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedTaxableIncome = 150_000m - (epf + 5_000m);
        var expectedPaye = CalculateProgressiveTax(expectedTaxableIncome);

        paySlip.Deductions.Should().Contain(d => d.Code == "DED_GROSS" && d.IsPreTax && d.Amount == 5_000m);
        paySlip.PayeTax.Should().Be(expectedPaye);
    }

    [Fact]
    public async Task CreatePayRun_Should_Lock_Overtime_Record_When_Included_In_PaySlip()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP023", "Kyle", 70_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        var overtime = TestDataSeeder.SeedOvertime(context.DbContext, employee, new DateOnly(2025, 4, 12), 4, OvertimeType.Weekend);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var persistedOvertime = await context.DbContext.OvertimeRecords.FindAsync(overtime.Id);
        persistedOvertime.Should().NotBeNull();
        persistedOvertime!.IsLockedForPayroll.Should().BeTrue();
        persistedOvertime.PayRunId.Should().Be(result.Id);
    }

    [Fact]
    public async Task CreatePayRun_Should_Use_Loan_Repayment_Schedule_When_Available()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP024", "Liam", 120_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        var loan = TestDataSeeder.SeedActiveLoan(context.DbContext, employee, 50_000m, 10_000m);
        TestDataSeeder.SeedLoanRepayment(context.DbContext, loan, new DateTime(2025, 4, 10), 6_000m);
        TestDataSeeder.SeedLoanRepayment(context.DbContext, loan, new DateTime(2025, 5, 10), 6_000m);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        paySlip.Deductions.Should().Contain(d => d.Code == "LOAN" && d.Amount == 6_000m);
        loan.OutstandingPrincipal.Should().Be(44_000m);
        loan.Repayments.Should().ContainSingle(r => r.IsPaid && r.Amount == 6_000m);
    }

    [Fact]
    public async Task CreatePayRun_Should_Apply_PreTax_Deduction_Before_Paye()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP013", "Ivy", 200_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var deductionType = TestDataSeeder.SeedDeductionType(context.DbContext, "DED_PRE", "Pre Tax Deduction", true, false);
        TestDataSeeder.SeedEmployeePayItem(context.DbContext, employee, PayItemType.Deduction, deductionType.Code, 10_000m, null);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var epf = Math.Round(200_000m * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedTaxableIncome = 200_000m - (epf + 10_000m);
        var expectedPaye = CalculateProgressiveTax(expectedTaxableIncome);

        paySlip.Deductions.Should().Contain(d => d.Code == "DED_PRE" && d.IsPreTax && d.Amount == 10_000m);
        paySlip.PayeTax.Should().Be(expectedPaye);
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
    public async Task CreatePayRun_Should_Apply_HalfDay_NoPay_Leave_Deduction()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP003A", "Cathy", 26_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        TestDataSeeder.SeedLeave(context.DbContext, employee, new DateOnly(2025, 4, 8), new DateOnly(2025, 4, 8), LeaveTypeCode.NoPay, 0.5, true);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var dailyRate = Math.Round(26_000m / 26m, 2, MidpointRounding.AwayFromZero);
        var expectedNoPay = Math.Round(dailyRate * 0.5m, 2, MidpointRounding.AwayFromZero);

        paySlip.Deductions.Should().Contain(d => d.Code == "LEAVE_NOPAY" && d.Amount == expectedNoPay);
        paySlip.Deductions.Should().NotContain(d => d.Code == "NOPAY");
    }

    [Fact]
    public async Task CreatePayRun_Should_Encash_Paid_Leave_When_Overlapping_Absence()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP003B", "Cris", 26_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);
        TestDataSeeder.SeedAbsence(context.DbContext, employee, new DateOnly(2025, 4, 12));
        TestDataSeeder.SeedLeave(context.DbContext, employee, new DateOnly(2025, 4, 12), new DateOnly(2025, 4, 12), LeaveTypeCode.Annual, 1);

        var payrollService = CreatePayrollService(context);
        var request = BuildDefaultRequest(employee.Id);

        var result = await payrollService.CreatePayRunAsync(request);

        var paySlip = result.PaySlips.Should().ContainSingle().Subject;
        var dailyRate = Math.Round(26_000m / 26m, 2, MidpointRounding.AwayFromZero);
        var expectedEpf = Math.Round(26_000m * 0.08m, 2, MidpointRounding.AwayFromZero);
        var expectedNet = Math.Round((26_000m + dailyRate) - (expectedEpf + dailyRate), 2, MidpointRounding.AwayFromZero);

        paySlip.Earnings.Should().Contain(e => e.Code == "LEAVE_ENCASH" && e.Amount == dailyRate && !e.IsEpfApplicable && !e.IsTaxable);
        paySlip.Deductions.Should().Contain(d => d.Code == "NOPAY" && d.Amount == dailyRate);
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

    [Fact]
    public async Task GenerateBankExport_Should_Return_Failures_When_Bank_Details_Missing()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP100", "Una", 120_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRun = await payrollService.CreatePayRunAsync(BuildDefaultRequest(employee.Id));

        var result = await payrollService.GenerateBankExportAsync(payRun.Id, new BankExportRequest { Bank = "HNB" });

        result.Failures.Should().ContainSingle(f => f.EmployeeId == employee.Id);
        result.Status.Should().Be(BankExportStatus.Pending);
    }

    [Fact]
    public async Task GenerateBankExport_Should_Create_File_And_Update_Status()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP101", "Vera", 150_000m);
        employee.UpdateBankDetails("HNB", "7080", "001", "1234567890");
        context.DbContext.SaveChanges();
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRun = await payrollService.CreatePayRunAsync(BuildDefaultRequest(employee.Id));

        var result = await payrollService.GenerateBankExportAsync(payRun.Id, new BankExportRequest { Bank = "BOC" });

        result.Failures.Should().BeEmpty();
        result.ContentBase64.Should().NotBeNullOrEmpty();
        result.Bank.Should().Be("BOC");

        var refreshed = await payrollService.GetPayRunAsync(payRun.Id);
        refreshed.Should().NotBeNull();
        refreshed!.ExportStatus.Should().Be(BankExportStatus.Generated);
        refreshed.ExportedBank.Should().Be("BOC");
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
