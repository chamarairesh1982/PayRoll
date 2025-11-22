using Payroll.Application.Interfaces;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Overtime;
using Payroll.Domain.PayrollConfig;
using Payroll.Domain.ValueObjects;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Application.Tests.TestInfrastructure;

public static class TestDataSeeder
{
    public static void SeedDefaultEpfEtfRule(PayrollDbContext context)
    {
        if (context.EpfEtfRuleSets.Any())
        {
            return;
        }

        var rule = new EpfEtfRuleSet
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test EPF/ETF Default",
            EffectiveFrom = new DateOnly(2020, 1, 1),
            EffectiveTo = null,
            EmployeeEpfRate = 8m,
            EmployerEpfRate = 12m,
            EmployerEtfRate = 3m,
            MinimumWageForEpf = null,
            MaximumEarningForEpf = null,
            MaximumEarningForEtf = null,
            IsDefault = true,
            IsActive = true,
            CreatedBy = "seed"
        };

        context.EpfEtfRuleSets.Add(rule);
        context.SaveChanges();
    }

    public static void SeedSimpleTaxRuleSet(PayrollDbContext context)
    {
        if (context.TaxRuleSets.Any())
        {
            return;
        }

        var taxRuleSetId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var ruleSet = new TaxRuleSet
        {
            Id = taxRuleSetId,
            Name = "Test Sri Lanka PAYE",
            YearOfAssessment = 2025,
            EffectiveFrom = new DateOnly(2025, 4, 1),
            EffectiveTo = null,
            IsDefault = true,
            IsActive = true,
            CreatedBy = "seed"
        };

        var slabs = new List<TaxSlab>
        {
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                TaxRuleSetId = taxRuleSetId,
                FromAmount = 0m,
                ToAmount = 100000m,
                RatePercent = 0m,
                Order = 1,
                CreatedBy = "seed"
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222224"),
                TaxRuleSetId = taxRuleSetId,
                FromAmount = 100000m,
                ToAmount = 141667m,
                RatePercent = 6m,
                Order = 2,
                CreatedBy = "seed"
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222225"),
                TaxRuleSetId = taxRuleSetId,
                FromAmount = 141667m,
                ToAmount = 183333m,
                RatePercent = 12m,
                Order = 3,
                CreatedBy = "seed"
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222226"),
                TaxRuleSetId = taxRuleSetId,
                FromAmount = 183333m,
                ToAmount = null,
                RatePercent = 18m,
                Order = 4,
                CreatedBy = "seed"
            }
        };

        context.TaxRuleSets.Add(ruleSet);
        context.TaxSlabs.AddRange(slabs);
        context.SaveChanges();
    }

    public static void SeedAllowanceAndDeductionTypes(PayrollDbContext context)
    {
        if (!context.AllowanceTypes.Any())
        {
            context.AllowanceTypes.Add(new AllowanceType
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "BASIC",
                Name = "Basic Salary",
                Basis = CalculationBasis.FixedAmount,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true,
                CreatedBy = "seed"
            });
        }

        if (!context.DeductionTypes.Any())
        {
            context.DeductionTypes.AddRange(new List<DeductionType>
            {
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444441"),
                    Code = "EPF_EE",
                    Name = "Employee EPF",
                    Basis = CalculationBasis.PercentageOfBasic,
                    IsPreTax = true,
                    IsPostTax = false,
                    CreatedBy = "seed"
                },
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444442"),
                    Code = "PAYE",
                    Name = "PAYE Tax",
                    Basis = CalculationBasis.FixedAmount,
                    IsPreTax = false,
                    IsPostTax = true,
                    CreatedBy = "seed"
                },
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444443"),
                    Code = "LOAN",
                    Name = "Loan Installment",
                    Basis = CalculationBasis.FixedAmount,
                    IsPreTax = true,
                    IsPostTax = false,
                    CreatedBy = "seed"
                },
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Code = "NOPAY",
                    Name = "No Pay",
                    Basis = CalculationBasis.FixedAmount,
                    IsPreTax = true,
                    IsPostTax = false,
                    CreatedBy = "seed"
                }
            });
        }

        context.SaveChanges();
    }

    public static Employee SeedEmployee(PayrollDbContext context, string code, string name, decimal basicSalary)
    {
        var employee = Employee.Create(
            code,
            name,
            "Test",
            $"{code}-NIC",
            new DateTime(1990, 1, 1),
            Gender.Male,
            MaritalStatus.Single,
            new DateTime(2020, 1, 1),
            basicSalary,
            null,
            name,
            null,
            null,
            "seed");

        context.Employees.Add(employee);
        context.SaveChanges();

        return employee;
    }

    public static AttendanceRecord SeedAbsence(PayrollDbContext context, Employee employee, DateOnly date)
    {
        var record = new AttendanceRecord
        {
            EmployeeId = employee.Id,
            Period = new DateRange(date.ToDateTime(TimeOnly.MinValue), date.ToDateTime(TimeOnly.MinValue)),
            HoursWorked = 0,
            CreatedBy = "seed"
        };

        context.AttendanceRecords.Add(record);
        context.SaveChanges();

        return record;
    }

    public static OvertimeRecord SeedOvertime(PayrollDbContext context, Employee employee, DateOnly date, double hours, OvertimeType type = OvertimeType.Weekday)
    {
        var overtime = new OvertimeRecord
        {
            EmployeeId = employee.Id,
            Date = date,
            Hours = hours,
            Type = type,
            Status = OvertimeStatus.Approved,
            ApprovedAt = DateTimeOffset.UtcNow,
            IsLockedForPayroll = false,
            CreatedBy = "seed"
        };

        context.OvertimeRecords.Add(overtime);
        context.SaveChanges();

        return overtime;
    }

    public static Loan SeedActiveLoan(PayrollDbContext context, Employee employee, decimal principal, decimal installmentAmount)
    {
        var loan = new Loan
        {
            EmployeeId = employee.Id,
            PrincipalAmount = principal,
            OutstandingPrincipal = principal,
            InstallmentAmount = installmentAmount,
            StartDate = new DateTime(2024, 1, 1),
            Status = LoanStatus.Active,
            CreatedBy = "seed"
        };

        context.Loans.Add(loan);
        context.SaveChanges();

        return loan;
    }
}

public class TestCurrentUserService : ICurrentUserService
{
    public string? UserId => "test-user";
    public string? UserName => "test-user";
}
