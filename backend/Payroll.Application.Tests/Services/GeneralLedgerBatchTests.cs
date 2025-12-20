using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Services;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Application.TimeReconciliation;
using Payroll.Domain.GeneralLedger;
using Payroll.Domain.Payroll;
using Payroll.Domain.Employees;
using Payroll.Application.Interfaces;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class GeneralLedgerBatchTests
{
    [Fact]
    public async Task GenerateGlBatch_Should_Block_When_PayRun_Not_Locked()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_GL_1", "Gloria", 80_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: false);

        var service = context.CreatePayrollService();

        var act = async () => await service.GenerateGlJournalBatchAsync(payRun.Id, false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("GL batches can only be generated after the pay run is locked.");
    }

    [Fact]
    public async Task GenerateGlBatch_Should_Fail_When_Mapping_Missing()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_GL_2", "Mila", 90_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);
        SeedGlAccounts(context.DbContext);
        SeedPartialGlMapping(context.DbContext);

        var service = context.CreatePayrollService();

        var act = async () => await service.GenerateGlJournalBatchAsync(payRun.Id, false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Missing GL mappings*");
    }

    [Fact]
    public async Task GenerateGlBatch_Should_Be_Balanced()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_GL_3", "Sana", 120_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);
        SeedGlAccounts(context.DbContext);
        SeedGlMappings(context.DbContext);

        var service = context.CreatePayrollService();
        var batch = await service.GenerateGlJournalBatchAsync(payRun.Id, false);

        batch.IsBalanced.Should().BeTrue();
        batch.TotalDebits.Should().Be(batch.TotalCredits);
        batch.Lines.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateGlBatch_Should_Return_Existing_When_Approved()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_GL_4", "Ari", 110_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);
        SeedGlAccounts(context.DbContext);
        SeedGlMappings(context.DbContext);

        var service = context.CreatePayrollService();
        var batch = await service.GenerateGlJournalBatchAsync(payRun.Id, false);
        context.CurrentUserService.Roles = new[] { "Finance" };
        var approved = await service.ApproveGlJournalBatchAsync(batch.Id, new GlJournalBatchActionRequest { Comment = "Approved" });

        context.CurrentUserService.Roles = new[] { "Maker" };
        var second = await service.GenerateGlJournalBatchAsync(payRun.Id, false);

        second.Id.Should().Be(approved.Id);
        context.DbContext.GlJournalBatches.Count().Should().Be(1);
    }

    [Fact]
    public async Task ExportGlBatch_Should_Mark_Exported_And_Return_Csv()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_GL_5", "Nico", 95_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);
        SeedGlAccounts(context.DbContext);
        SeedGlMappings(context.DbContext);

        var service = context.CreatePayrollService();
        var batch = await service.GenerateGlJournalBatchAsync(payRun.Id, false);

        context.CurrentUserService.Roles = new[] { "Finance" };
        await service.ApproveGlJournalBatchAsync(batch.Id, new GlJournalBatchActionRequest());
        var export = await service.ExportGlJournalBatchAsync(batch.Id, "csv");

        export.Should().NotBeNull();
        var csv = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(export!.ContentBase64));
        csv.Should().Contain("PostingDate,AccountCode,AccountName,Description,Debit,Credit,CostCenter,Reference");

        var reloaded = await context.DbContext.GlJournalBatches.AsNoTracking().FirstAsync(b => b.Id == batch.Id);
        reloaded.Status.Should().Be(GlJournalBatchStatus.Exported);
    }

    private static PayRun SeedPayRun(Payroll.Infrastructure.Persistence.PayrollDbContext context, Employee employee, bool locked)
    {
        var payRun = new PayRun
        {
            Id = Guid.NewGuid(),
            Code = $"PR-{Guid.NewGuid():N}".Substring(0, 10),
            Name = "GL Test Pay Run",
            Reference = "GL-TEST",
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = locked ? PayRunStatus.Locked : PayRunStatus.Draft,
            IsLocked = locked,
            CreatedBy = "seed"
        };

        var paySlip = new PaySlip
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            EmployeeId = employee.Id,
            BasicSalary = employee.BaseSalary,
            TotalEarnings = 100_000m,
            TotalDeductions = 10_000m,
            NetPay = 90_000m,
            EmployerEpf = 12_000m,
            EmployerEtf = 3_000m,
            CreatedBy = "seed",
            Employee = employee
        };

        paySlip.Earnings.Add(new EarningLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = paySlip.Id,
            Code = "BASIC",
            Description = "Basic Salary",
            Amount = 100_000m
        });

        paySlip.Deductions.Add(new DeductionLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = paySlip.Id,
            Code = "PAYE",
            Description = "PAYE",
            Amount = 10_000m
        });

        payRun.PaySlips.Add(paySlip);
        context.PayRuns.Add(payRun);
        context.PaySlips.Add(paySlip);
        context.SaveChanges();
        return payRun;
    }

    private static void SeedGlAccounts(Payroll.Infrastructure.Persistence.PayrollDbContext context)
    {
        if (context.GlAccounts.Any())
        {
            return;
        }

        context.GlAccounts.AddRange(
            new GlAccount { Code = "SAL_EXP", Name = "Salary Expense", Type = GlAccountType.Expense, IsActive = true, CreatedBy = "seed" },
            new GlAccount { Code = "PAYROLL_PAYABLE", Name = "Payroll Payable", Type = GlAccountType.Liability, IsActive = true, CreatedBy = "seed" },
            new GlAccount { Code = "PAYE_PAYABLE", Name = "PAYE Payable", Type = GlAccountType.Liability, IsActive = true, CreatedBy = "seed" },
            new GlAccount { Code = "EPF_EXP", Name = "Employer EPF Expense", Type = GlAccountType.Expense, IsActive = true, CreatedBy = "seed" },
            new GlAccount { Code = "EPF_PAYABLE", Name = "EPF Payable", Type = GlAccountType.Liability, IsActive = true, CreatedBy = "seed" },
            new GlAccount { Code = "ETF_EXP", Name = "Employer ETF Expense", Type = GlAccountType.Expense, IsActive = true, CreatedBy = "seed" }
        );
        context.SaveChanges();
    }

    private static void SeedGlMappings(Payroll.Infrastructure.Persistence.PayrollDbContext context)
    {
        if (context.GlMappings.Any())
        {
            return;
        }

        var accounts = context.GlAccounts.ToDictionary(a => a.Code, a => a.Id);

        context.GlMappings.AddRange(
            new GlMapping
            {
                PayComponentCode = "BASIC",
                PayComponentType = GlPayComponentType.Earning,
                DebitAccountId = accounts["SAL_EXP"],
                CreditAccountId = accounts["PAYROLL_PAYABLE"],
                PostingSideRule = GlPostingSideRule.DebitWhenPositive,
                CreatedBy = "seed"
            },
            new GlMapping
            {
                PayComponentCode = "PAYE",
                PayComponentType = GlPayComponentType.Deduction,
                DebitAccountId = accounts["PAYROLL_PAYABLE"],
                CreditAccountId = accounts["PAYE_PAYABLE"],
                PostingSideRule = GlPostingSideRule.DebitWhenPositive,
                CreatedBy = "seed"
            },
            new GlMapping
            {
                PayComponentCode = "EPF_EMPLOYER",
                PayComponentType = GlPayComponentType.EmployerContribution,
                DebitAccountId = accounts["EPF_EXP"],
                CreditAccountId = accounts["EPF_PAYABLE"],
                PostingSideRule = GlPostingSideRule.DebitWhenPositive,
                CreatedBy = "seed"
            },
            new GlMapping
            {
                PayComponentCode = "ETF_EMPLOYER",
                PayComponentType = GlPayComponentType.EmployerContribution,
                DebitAccountId = accounts["ETF_EXP"],
                CreditAccountId = accounts["EPF_PAYABLE"],
                PostingSideRule = GlPostingSideRule.DebitWhenPositive,
                CreatedBy = "seed"
            }
        );

        context.SaveChanges();
    }

    private static void SeedPartialGlMapping(Payroll.Infrastructure.Persistence.PayrollDbContext context)
    {
        if (context.GlMappings.Any())
        {
            return;
        }

        var accounts = context.GlAccounts.ToDictionary(a => a.Code, a => a.Id);

        context.GlMappings.Add(new GlMapping
        {
            PayComponentCode = "PAYE",
            PayComponentType = GlPayComponentType.Deduction,
            DebitAccountId = accounts["PAYROLL_PAYABLE"],
            CreditAccountId = accounts["PAYE_PAYABLE"],
            PostingSideRule = GlPostingSideRule.DebitWhenPositive,
            CreatedBy = "seed"
        });

        context.SaveChanges();
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService { Roles = new[] { "Maker" } };
        }

        public Payroll.Infrastructure.Persistence.PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }

        public PayrollService CreatePayrollService()
        {
            var auditLogger = new AuditLogger(DbContext, CurrentUserService);
            var epfService = new Payroll.Application.PayrollConfig.EpfEtfRuleSetService(DbContext, CurrentUserService);
            var taxService = new Payroll.Application.PayrollConfig.TaxRuleSetService(DbContext, CurrentUserService);
            return new PayrollService(
                DbContext,
                epfService,
                taxService,
                auditLogger,
                CurrentUserService,
                new TimeReconciliationService());
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; } = "user-1";
        public string? UserName { get; set; } = "GL Tester";
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    }
}
