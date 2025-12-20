using System.Text;
using System.Text.Json;
using FluentAssertions;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig;
using Payroll.Application.Services;
using Payroll.Application.Tests.Services;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Application.TimeReconciliation;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Xunit;

namespace Payroll.Application.Tests.TaxDocuments;

public class TaxDocumentServiceTests
{
    [Fact]
    public async Task GenerateMonthlyReport_Should_Aggregate_Across_Multiple_PayRuns()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_TAX", "Lina", 200_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRunA = await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 4, 1), new DateTime(2025, 4, 15), new DateTime(2025, 4, 15)));
        var payRunB = await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 4, 16), new DateTime(2025, 4, 30), new DateTime(2025, 4, 30)));

        await LockPayRunAsync(context, payRunA.Id);
        await LockPayRunAsync(context, payRunB.Id);

        var service = new TaxDocumentService(context.DbContext, context.CurrentUserService, context.Storage);
        var result = await service.GenerateMonthlyReportAsync(new MonthlyTaxReportRequestDto
        {
            Year = 2025,
            Month = 4,
            Format = "csv"
        }, regenerate: false);

        result.Type.Should().Be(GeneratedTaxDocumentType.MonthlyReport);

        var slips = context.DbContext.PaySlips.Where(ps => ps.EmployeeId == employee.Id).ToList();
        var expectedTax = slips.Sum(ps => ps.PayeTax);

        var document = context.DbContext.GeneratedTaxDocuments.Single(d => d.Id == result.Id);
        var csv = await context.Storage.ReadTextAsync(document.FilePath!);
        csv.Should().Contain(expectedTax.ToString("F2"));
    }

    [Fact]
    public async Task GenerateAnnualReport_Should_Summarize_Year_Totals()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_YEAR", "Nila", 180_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRunApril = await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 4, 1), new DateTime(2025, 4, 30), new DateTime(2025, 4, 30)));
        var payRunMay = await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 5, 1), new DateTime(2025, 5, 31), new DateTime(2025, 5, 31)));

        await LockPayRunAsync(context, payRunApril.Id);
        await LockPayRunAsync(context, payRunMay.Id);

        var service = new TaxDocumentService(context.DbContext, context.CurrentUserService, context.Storage);
        var result = await service.GenerateAnnualReportAsync(new AnnualTaxReportRequestDto
        {
            Year = 2025,
            Format = "csv"
        }, regenerate: false);

        var slips = context.DbContext.PaySlips.Where(ps => ps.EmployeeId == employee.Id).ToList();
        var expectedTaxable = slips.Sum(ps => ExtractTaxableEarnings(ps));

        var document = context.DbContext.GeneratedTaxDocuments.Single(d => d.Id == result.Id);
        var csv = await context.Storage.ReadTextAsync(document.FilePath!);
        csv.Should().Contain(expectedTaxable.ToString("F2"));
    }

    [Fact]
    public async Task GenerateCertificate_Should_Store_Checksum_And_Totals()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_CERT", "Amaya", 160_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRun = await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 4, 1), new DateTime(2025, 4, 30), new DateTime(2025, 4, 30)));
        await LockPayRunAsync(context, payRun.Id);

        var service = new TaxDocumentService(context.DbContext, context.CurrentUserService, context.Storage);
        var result = await service.GenerateEmployeeCertificateAsync(new TaxCertificateRequestDto
        {
            EmployeeId = employee.Id,
            Year = 2025
        }, regenerate: false);

        var document = context.DbContext.GeneratedTaxDocuments.Single(d => d.Id == result.Id);
        document.ChecksumSha256.Should().NotBeNullOrWhiteSpace();
        document.Status.Should().Be(GeneratedTaxDocumentStatus.Generated);

        var metadata = JsonSerializer.Deserialize<CertificateMetadata>(document.MetadataJson!);
        metadata.Should().NotBeNull();
        metadata!.Totals.ApitDeducted.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateMonthlyReport_Should_Reject_When_No_Locked_PayRuns()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_OPEN", "Ravi", 140_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 4, 1), new DateTime(2025, 4, 30), new DateTime(2025, 4, 30)));

        var service = new TaxDocumentService(context.DbContext, context.CurrentUserService, context.Storage);

        var action = () => service.GenerateMonthlyReportAsync(new MonthlyTaxReportRequestDto
        {
            Year = 2025,
            Month = 4,
            Format = "csv"
        }, regenerate: false);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No locked pay runs found for the selected month.");
    }

    [Fact]
    public async Task GenerateMonthlyReport_Should_Be_Idempotent_Unless_Regenerate()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_IDEMP", "Mila", 150_000m);
        TestDataSeeder.SeedDefaultEpfEtfRule(context.DbContext);
        TestDataSeeder.SeedSimpleTaxRuleSet(context.DbContext);

        var payrollService = CreatePayrollService(context);
        var payRun = await payrollService.CreatePayRunAsync(BuildRequest(employee.Id, new DateTime(2025, 4, 1), new DateTime(2025, 4, 30), new DateTime(2025, 4, 30)));
        await LockPayRunAsync(context, payRun.Id);

        var service = new TaxDocumentService(context.DbContext, context.CurrentUserService, context.Storage);
        var first = await service.GenerateMonthlyReportAsync(new MonthlyTaxReportRequestDto
        {
            Year = 2025,
            Month = 4,
            Format = "csv"
        }, regenerate: false);

        var second = await service.GenerateMonthlyReportAsync(new MonthlyTaxReportRequestDto
        {
            Year = 2025,
            Month = 4,
            Format = "csv"
        }, regenerate: false);

        first.Id.Should().Be(second.Id);
        context.DbContext.GeneratedTaxDocuments.Count().Should().Be(1);

        var regenerated = await service.GenerateMonthlyReportAsync(new MonthlyTaxReportRequestDto
        {
            Year = 2025,
            Month = 4,
            Format = "csv"
        }, regenerate: true);

        regenerated.Id.Should().NotBe(first.Id);
        context.DbContext.GeneratedTaxDocuments.Count().Should().Be(2);
    }

    private static PayrollService CreatePayrollService(TestContext context)
    {
        var epfService = new EpfEtfRuleSetService(context.DbContext, new FakeCurrentUserService());
        var taxService = new TaxRuleSetService(context.DbContext, new FakeCurrentUserService());
        var auditLogger = new AuditLogger(context.DbContext, new FakeCurrentUserService());
        var timeReconciliationService = new TimeReconciliationService();
        return new PayrollService(
            context.DbContext,
            epfService,
            taxService,
            auditLogger,
            new FakeCurrentUserService(),
            timeReconciliationService);
    }

    private static CreatePayRunRequest BuildRequest(Guid employeeId, DateTime periodStart, DateTime periodEnd, DateTime payDate)
    {
        return new CreatePayRunRequest
        {
            Name = $"Run {periodStart:yyyyMMdd}",
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PayDate = payDate,
            EmployeeIds = new List<Guid> { employeeId },
            IncludeActiveEmployeesOnly = true
        };
    }

    private static async Task LockPayRunAsync(TestContext context, Guid payRunId)
    {
        var payRun = await context.DbContext.PayRuns.FindAsync(payRunId);
        payRun!.Status = PayRunStatus.Locked;
        payRun.IsLocked = true;
        await context.DbContext.SaveChangesAsync();
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new AdminCurrentUserService();
            Storage = new InMemoryTaxDocumentStorage();
            TestDataSeeder.SeedAllowanceAndDeductionTypes(DbContext);
        }

        public Payroll.Infrastructure.Persistence.PayrollDbContext DbContext { get; }
        public AdminCurrentUserService CurrentUserService { get; }
        public InMemoryTaxDocumentStorage Storage { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private sealed class AdminCurrentUserService : ICurrentUserService
    {
        public string? UserId => "admin-user";
        public string? UserName => "Admin User";
        public IReadOnlyCollection<string> Roles => new[] { "Admin" };
    }

    private sealed class InMemoryTaxDocumentStorage : ITaxDocumentStorage
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

        public async Task<string> ReadTextAsync(string filePath)
        {
            var bytes = await ReadAsync(filePath);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    private sealed class CertificateMetadata
    {
        public CertificateTotals Totals { get; set; } = new();
    }

    private sealed class CertificateTotals
    {
        public decimal ApitDeducted { get; set; }
    }

    private static decimal ExtractTaxableEarnings(PaySlip paySlip)
    {
        if (!string.IsNullOrWhiteSpace(paySlip.TaxCalculationJson))
        {
            var summary = JsonSerializer.Deserialize<TaxSummary>(paySlip.TaxCalculationJson);
            if (summary != null)
            {
                return summary.TaxableEarnings;
            }
        }

        return paySlip.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
    }

    private sealed class TaxSummary
    {
        public decimal TaxableEarnings { get; set; }
    }
}
