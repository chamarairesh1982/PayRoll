using System.Text;
using System.Text.Json;
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

public class PayslipDocumentServiceTests
{
    [Fact]
    public async Task Generate_Should_Require_Locked_PayRun()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP301", "Nila", 95_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: false);

        var service = new PayslipDocumentService(context.DbContext, context.Storage, context.CurrentUserService);

        var act = () => service.GenerateAsync(payRun.Id, employee.Id, false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payslips can only be generated after the pay run is locked.");
    }

    [Fact]
    public async Task Generate_Should_Create_File_And_Metadata()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP302", "Kala", 120_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);

        var service = new PayslipDocumentService(context.DbContext, context.Storage, context.CurrentUserService);
        var result = await service.GenerateAsync(payRun.Id, employee.Id, false);

        result.Should().NotBeNull();
        result!.Status.Should().Be(PayslipDocumentStatus.Generated);
        result.FileName.Should().NotBeNullOrWhiteSpace();
        context.Storage.Files.Should().ContainKey(result.FileName!);
    }

    [Fact]
    public async Task Generate_Should_Store_Checksum()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP303", "Sachi", 110_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);

        var service = new PayslipDocumentService(context.DbContext, context.Storage, context.CurrentUserService);
        var result = await service.GenerateAsync(payRun.Id, employee.Id, false);

        var document = context.DbContext.PayslipDocuments.Single();
        document.ChecksumSha256.Should().NotBeNullOrWhiteSpace();

        var paySlip = context.DbContext.PaySlips.Single(ps => ps.EmployeeId == employee.Id);
        var payload = BuildPayload(payRun, paySlip, employee);
        var expected = ComputeChecksum(payload);

        document.ChecksumSha256.Should().Be(expected);
    }

    [Fact]
    public async Task Generate_Should_Be_Idempotent()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP304", "Iresha", 105_000m);
        var payRun = SeedPayRun(context.DbContext, employee, locked: true);

        var service = new PayslipDocumentService(context.DbContext, context.Storage, context.CurrentUserService);
        var first = await service.GenerateAsync(payRun.Id, employee.Id, false);
        var second = await service.GenerateAsync(payRun.Id, employee.Id, false);

        first!.Id.Should().Be(second!.Id);
        context.DbContext.PayslipDocuments.Count().Should().Be(1);

        var regenerated = await service.GenerateAsync(payRun.Id, employee.Id, true);
        regenerated!.Id.Should().NotBe(first.Id);
        context.DbContext.PayslipDocuments.Count().Should().Be(2);
    }

    [Fact]
    public async Task GenerateBulk_Should_Return_Summary()
    {
        using var context = new TestContext();
        var employeeOne = TestDataSeeder.SeedEmployee(context.DbContext, "EMP305", "Dilan", 90_000m);
        var employeeTwo = TestDataSeeder.SeedEmployee(context.DbContext, "EMP306", "Shani", 115_000m);
        var payRun = SeedPayRun(context.DbContext, employeeOne, locked: true);
        SeedPaySlip(context.DbContext, payRun, employeeTwo);

        var service = new PayslipDocumentService(context.DbContext, context.Storage, context.CurrentUserService);
        await service.GenerateAsync(payRun.Id, employeeOne.Id, false);

        var result = await service.GenerateBulkAsync(payRun.Id);

        result.GeneratedCount.Should().Be(1);
        result.SkippedCount.Should().Be(1);
        result.FailedCount.Should().Be(0);
    }

    private static PayRun SeedPayRun(PayrollDbContext context, Employee employee, bool locked)
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

        SeedPaySlip(context, payRun, employee);
        context.PayRuns.Add(payRun);
        context.SaveChanges();
        return payRun;
    }

    private static void SeedPaySlip(PayrollDbContext context, PayRun payRun, Employee employee)
    {
        var paySlip = new PaySlip
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            EmployeeId = employee.Id,
            BasicSalary = employee.BaseSalary,
            TotalEarnings = employee.BaseSalary + 5_000m,
            TotalDeductions = 2_000m,
            NetPay = employee.BaseSalary + 3_000m,
            EmployeeEpf = 1_600m,
            EmployerEpf = 2_400m,
            EmployerEtf = 600m,
            PayeTax = 500m,
            CreatedBy = "seed",
            Employee = employee
        };

        paySlip.Earnings.Add(new EarningLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = paySlip.Id,
            Code = "BASIC",
            Description = "Basic Salary",
            Amount = employee.BaseSalary
        });

        paySlip.Earnings.Add(new EarningLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = paySlip.Id,
            Code = "ALLOW",
            Description = "Allowance",
            Amount = 5_000m
        });

        paySlip.Deductions.Add(new DeductionLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = paySlip.Id,
            Code = "EPF",
            Description = "EPF",
            Amount = 1_600m
        });

        context.PaySlips.Add(paySlip);
        payRun.PaySlips.Add(paySlip);
    }

    private static PayslipDocumentPayload BuildPayload(PayRun payRun, PaySlip paySlip, Employee employee)
    {
        return new PayslipDocumentPayload
        {
            PayRunId = payRun.Id,
            EmployeeId = paySlip.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate,
            GrossPay = paySlip.TotalEarnings,
            TotalDeductions = paySlip.TotalDeductions,
            NetPay = paySlip.NetPay,
            EmployeeEpf = paySlip.EmployeeEpf,
            EmployerEpf = paySlip.EmployerEpf,
            EmployerEtf = paySlip.EmployerEtf,
            PayeTax = paySlip.PayeTax,
            Earnings = paySlip.Earnings
                .OrderBy(e => e.Code)
                .Select(e => new PayslipDocumentLine
                {
                    Code = e.Code,
                    Description = e.Description,
                    Amount = e.Amount
                })
                .ToList(),
            Deductions = paySlip.Deductions
                .OrderBy(d => d.Code)
                .Select(d => new PayslipDocumentLine
                {
                    Code = d.Code,
                    Description = d.Description,
                    Amount = d.Amount
                })
                .ToList()
        };
    }

    private static string ComputeChecksum(PayslipDocumentPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService();
            Storage = new InMemoryPayslipDocumentStorage();
        }

        public PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }
        public InMemoryPayslipDocumentStorage Storage { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private sealed class InMemoryPayslipDocumentStorage : IPayslipDocumentStorage
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
