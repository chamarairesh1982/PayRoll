using FluentAssertions;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.Services;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;
using Payroll.Infrastructure.Persistence;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class BankExportServiceTests
{
    [Fact]
    public async Task GenerateExport_Should_Require_Locked_PayRun()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP201", "Lena", 120_000m);
        employee.UpdateBankDetails("HNB", "7083", "001", "12345678");
        context.DbContext.SaveChanges();
        TestDataSeeder.SeedBankExportTemplate(context.DbContext);
        var payRun = SeedPayRun(context.DbContext, employee, 100_000m, false);

        var service = new BankExportService(context.DbContext, context.CurrentUserService, context.Storage);

        var act = () => service.GenerateExportAsync(payRun.Id, new BankExportGenerateRequest { TemplateId = context.TemplateId }, false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Bank exports can only be generated for locked pay runs.");
    }

    [Fact]
    public async Task GenerateExport_Should_Return_Validation_Errors()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP202", "Maya", 120_000m);
        employee.UpdateBankDetails("HNB", "7083", "001", "ABC");
        context.DbContext.SaveChanges();
        TestDataSeeder.SeedBankExportTemplate(context.DbContext);
        var payRun = SeedPayRun(context.DbContext, employee, 100_000m, true);

        var service = new BankExportService(context.DbContext, context.CurrentUserService, context.Storage);
        var result = await service.GenerateExportAsync(
            payRun.Id,
            new BankExportGenerateRequest { TemplateId = context.TemplateId },
            false);

        result.Export.Status.Should().Be(BankExportStatus.Failed);
        result.ValidationErrors.Should().NotBeEmpty();
        var export = context.DbContext.PayRunBankExports.Single();
        export.Status.Should().Be(BankExportStatus.Failed);
    }

    [Fact]
    public async Task GenerateExport_Should_Create_File_And_Metadata()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP203", "Nadia", 120_000m);
        employee.UpdateBankDetails("HNB", "7083", "001", "1234567890");
        context.DbContext.SaveChanges();
        TestDataSeeder.SeedBankExportTemplate(context.DbContext);
        var payRun = SeedPayRun(context.DbContext, employee, 100_000m, true);

        var service = new BankExportService(context.DbContext, context.CurrentUserService, context.Storage);
        var result = await service.GenerateExportAsync(
            payRun.Id,
            new BankExportGenerateRequest { TemplateId = context.TemplateId },
            false);

        result.Export.Status.Should().Be(BankExportStatus.Generated);
        result.Export.FileName.Should().NotBeNullOrWhiteSpace();
        context.Storage.Files.Should().ContainKey(result.Export.FileName!);
        var content = System.Text.Encoding.UTF8.GetString(context.Storage.Files[result.Export.FileName!]);
        content.Should().Contain("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");
        content.Should().Contain("Nadia");
    }

    [Fact]
    public async Task DownloadExport_Should_Mark_Downloaded()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP204", "Omi", 120_000m);
        employee.UpdateBankDetails("HNB", "7083", "001", "1234567890");
        context.DbContext.SaveChanges();
        TestDataSeeder.SeedBankExportTemplate(context.DbContext);
        var payRun = SeedPayRun(context.DbContext, employee, 100_000m, true);

        var service = new BankExportService(context.DbContext, context.CurrentUserService, context.Storage);
        var result = await service.GenerateExportAsync(
            payRun.Id,
            new BankExportGenerateRequest { TemplateId = context.TemplateId },
            false);

        var file = await service.DownloadExportAsync(result.Export.Id);
        file.Should().NotBeNull();

        var export = context.DbContext.PayRunBankExports.Single();
        export.Status.Should().Be(BankExportStatus.Downloaded);
        export.DownloadedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateExport_Should_Be_Idempotent_When_Not_Regenerating()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP205", "Pavi", 120_000m);
        employee.UpdateBankDetails("HNB", "7083", "001", "1234567890");
        context.DbContext.SaveChanges();
        TestDataSeeder.SeedBankExportTemplate(context.DbContext);
        var payRun = SeedPayRun(context.DbContext, employee, 100_000m, true);

        var service = new BankExportService(context.DbContext, context.CurrentUserService, context.Storage);
        var first = await service.GenerateExportAsync(
            payRun.Id,
            new BankExportGenerateRequest { TemplateId = context.TemplateId },
            false);
        var second = await service.GenerateExportAsync(
            payRun.Id,
            new BankExportGenerateRequest { TemplateId = context.TemplateId },
            false);

        first.Export.Id.Should().Be(second.Export.Id);
        context.DbContext.PayRunBankExports.Count().Should().Be(1);

        var third = await service.GenerateExportAsync(
            payRun.Id,
            new BankExportGenerateRequest { TemplateId = context.TemplateId },
            true);

        third.Export.Id.Should().NotBe(first.Export.Id);
        context.DbContext.PayRunBankExports.Count().Should().Be(2);
    }

    private static PayRun SeedPayRun(PayrollDbContext context, Employee employee, decimal netPay, bool locked)
    {
        var payRun = new PayRun
        {
            Id = Guid.NewGuid(),
            Code = $"PR-{Guid.NewGuid():N}".Substring(0, 10),
            Name = "Test Pay Run",
            Reference = "PR-TEST",
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
            TotalEarnings = netPay,
            TotalDeductions = 0,
            NetPay = netPay,
            CreatedBy = "seed",
            Employee = employee
        };

        payRun.PaySlips.Add(paySlip);
        context.PayRuns.Add(payRun);
        context.PaySlips.Add(paySlip);
        context.SaveChanges();
        return payRun;
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService();
            Storage = new InMemoryBankExportStorage();
            var template = TestDataSeeder.SeedBankExportTemplate(DbContext);
            TemplateId = template.Id;
        }

        public PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }
        public InMemoryBankExportStorage Storage { get; }
        public Guid TemplateId { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private sealed class InMemoryBankExportStorage : IBankExportStorage
    {
        public Dictionary<string, byte[]> Files { get; } = new();

        public Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
        {
            Files[fileName] = content;
            return Task.FromResult(fileName);
        }

        public Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!Files.TryGetValue(filePath, out var content))
            {
                throw new FileNotFoundException();
            }

            return Task.FromResult(content);
        }
    }
}
