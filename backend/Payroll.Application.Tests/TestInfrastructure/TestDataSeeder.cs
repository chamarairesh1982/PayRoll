using Payroll.Application.Interfaces;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
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

    public static AllowanceType SeedAllowanceType(
        PayrollDbContext context,
        string code,
        string name,
        bool isTaxable = true,
        bool isEpfApplicable = true,
        bool isEtfApplicable = true)
    {
        var existing = context.AllowanceTypes.FirstOrDefault(a => a.Code == code);
        if (existing is not null)
        {
            return existing;
        }

        var allowanceType = new AllowanceType
        {
            Code = code,
            Name = name,
            Basis = CalculationBasis.FixedAmount,
            IsTaxable = isTaxable,
            IsEpfApplicable = isEpfApplicable,
            IsEtfApplicable = isEtfApplicable,
            IsActive = true,
            CreatedBy = "seed"
        };

        context.AllowanceTypes.Add(allowanceType);
        context.SaveChanges();

        return allowanceType;
    }

    public static DeductionType SeedDeductionType(
        PayrollDbContext context,
        string code,
        string name,
        bool isPreTax = true,
        bool isPostTax = false)
    {
        var existing = context.DeductionTypes.FirstOrDefault(d => d.Code == code);
        if (existing is not null)
        {
            return existing;
        }

        var deductionType = new DeductionType
        {
            Code = code,
            Name = name,
            Basis = CalculationBasis.FixedAmount,
            IsPreTax = isPreTax,
            IsPostTax = isPostTax,
            IsActive = true,
            CreatedBy = "seed"
        };

        context.DeductionTypes.Add(deductionType);
        context.SaveChanges();

        return deductionType;
    }

    public static Employee SeedEmployee(PayrollDbContext context, string code, string name, decimal basicSalary)
    {
        var joinDate = new DateTime(2020, 1, 1);
        var probationEndDate = joinDate.AddMonths(3); // ✅ domain-safe default

        var employee = Employee.Create(
                employeeCode: code,
                firstName: name,
                lastName: "Test",
                nicNumber: $"{code}-NIC",
                epfNumber: null,
                dateOfBirth: new DateTime(1990, 1, 1),
                gender: Gender.Male,
                maritalStatus: MaritalStatus.Single,
                employmentStartDate: joinDate,
                baseSalary: basicSalary,
                companyId: null,
                branchId: null,
                costCenterId: null,
                initials: null,
                callingName: null,
                probationEndDate: probationEndDate,   // ✅ correct position
                confirmationDate: null,
                createdBy: "seed",
                bankName: null,
                bankCode: null,
                branchCode: null,
                bankAccountNumber: null
            );

        context.Employees.Add(employee);
        context.SaveChanges();

        return employee;
    }

    public static AttendanceRecord SeedAbsence(PayrollDbContext context, Employee employee, DateOnly date)
    {
        var record = new AttendanceRecord
        {
            EmployeeId = employee.Id,
            Period = new DateRange(date, date),
            HoursWorked = 0,
            CreatedBy = "seed"
        };

        context.AttendanceRecords.Add(record);
        context.SaveChanges();

        return record;
    }

    public static AttendanceRecord SeedAttendance(PayrollDbContext context, Employee employee, DateOnly date, decimal hoursWorked)
    {
        var record = new AttendanceRecord
        {
            EmployeeId = employee.Id,
            Period = new DateRange(date, date),
            HoursWorked = hoursWorked,
            CreatedBy = "seed"
        };

        context.AttendanceRecords.Add(record);
        context.SaveChanges();

        return record;
    }

    public static LeaveRequest SeedLeave(
        PayrollDbContext context,
        Employee employee,
        DateOnly startDate,
        DateOnly endDate,
        LeaveTypeCode leaveType,
        double totalDays,
        bool isHalfDay = false)
    {
        var leave = new LeaveRequest
        {
            EmployeeId = employee.Id,
            StartDate = startDate,
            EndDate = endDate,
            LeaveType = leaveType,
            TotalDays = totalDays,
            Status = LeaveStatus.Approved,
            IsHalfDay = isHalfDay,
            CreatedBy = "seed",
            RequestedAt = DateTimeOffset.UtcNow,
            ApprovedAt = DateTimeOffset.UtcNow
        };

        context.LeaveRequests.Add(leave);
        context.SaveChanges();

        return leave;
    }

    public static OTEntry SeedOvertime(PayrollDbContext context, Employee employee, DateOnly date, double hours, OvertimeType type = OvertimeType.Weekday)
    {
        var overtime = new OTEntry
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

        context.OTEntries.Add(overtime);
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

    public static LoanRepayment SeedLoanRepayment(PayrollDbContext context, Loan loan, DateTime dueDate, decimal amount, bool isPaid = false)
    {
        var repayment = new LoanRepayment
        {
            LoanId = loan.Id,
            DueDate = dueDate,
            Amount = amount,
            IsPaid = isPaid
        };

        context.LoanRepayments.Add(repayment);
        context.SaveChanges();

        loan.Repayments.Add(repayment);

        return repayment;
    }

    public static EmployeePayItem SeedEmployeePayItem(
        PayrollDbContext context,
        Employee employee,
        PayItemType payItemType,
        string payItemCode,
        decimal? amount,
        decimal? percentage,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null)
    {
        if ((amount.HasValue && percentage.HasValue) || (!amount.HasValue && !percentage.HasValue))
        {
            throw new ArgumentException("Exactly one of amount or percentage must be provided.");
        }

        if (amount.HasValue && amount.Value <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        if (percentage.HasValue && percentage.Value <= 0)
        {
            throw new ArgumentException("Percentage must be greater than zero.");
        }

        var start = effectiveFrom ?? new DateOnly(2025, 4, 1);
        var end = effectiveTo ?? DateOnly.MaxValue;

        var overlapExists = context.EmployeePayItems.Any(pi =>
            pi.EmployeeId == employee.Id
            && pi.PayItemType == payItemType
            && pi.PayItemCode == payItemCode
            && pi.IsActive
            && pi.EffectiveFrom <= end
            && (pi.EffectiveTo == null || pi.EffectiveTo >= start));

        if (overlapExists)
        {
            throw new InvalidOperationException("Overlapping pay item range detected for employee");
        }

        var payItem = new EmployeePayItem
        {
            EmployeeId = employee.Id,
            PayItemType = payItemType,
            PayItemCode = payItemCode,
            Amount = amount,
            Percentage = percentage,
            EffectiveFrom = start,
            EffectiveTo = effectiveTo,
            IsActive = true,
            CreatedBy = "seed"
        };

        context.EmployeePayItems.Add(payItem);
        context.SaveChanges();

        return payItem;
    }

    public static RecurringPayItemRule SeedRecurringPayItemRule(
        PayrollDbContext context,
        string name,
        RecurringRuleType ruleType,
        Guid payComponentId,
        decimal amount,
        DateOnly startDate,
        DateOnly? endDate,
        bool taxable,
        bool epfEtfContributable,
        bool prorate,
        bool isActive = true)
    {
        var existing = context.RecurringPayItemRules.FirstOrDefault(r => r.Name == name);
        if (existing is not null)
        {
            return existing;
        }

        var rule = new RecurringPayItemRule
        {
            Name = name,
            RuleType = ruleType,
            AllowanceTypeId = ruleType == RecurringRuleType.Allowance ? payComponentId : null,
            DeductionTypeId = ruleType == RecurringRuleType.Deduction ? payComponentId : null,
            Amount = amount,
            Frequency = PayPeriodType.Monthly,
            StartDate = startDate,
            EndDate = endDate,
            Taxable = taxable,
            EpfEtfContributable = epfEtfContributable,
            Prorate = prorate,
            IsActive = isActive,
            CreatedBy = "seed"
        };

        context.RecurringPayItemRules.Add(rule);
        context.SaveChanges();

        return rule;
    }

    public static RecurringPayItemAssignment SeedRecurringPayItemAssignment(
        PayrollDbContext context,
        Guid ruleId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate,
        bool isActive = true)
    {
        var existing = context.RecurringPayItemAssignments.FirstOrDefault(a =>
            a.RuleId == ruleId
            && a.EmployeeId == employeeId
            && a.StartDate == startDate);

        if (existing is not null)
        {
            return existing;
        }

        var assignment = new RecurringPayItemAssignment
        {
            RuleId = ruleId,
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = isActive,
            CreatedBy = "seed"
        };

        context.RecurringPayItemAssignments.Add(assignment);
        context.SaveChanges();

        return assignment;
    }
}

public class TestCurrentUserService : ICurrentUserService
{
    public string? UserId => "test-user";
    public string? UserName => "test-user";
    public IReadOnlyCollection<string> Roles => new[] { "Maker", "Approver" };
}
