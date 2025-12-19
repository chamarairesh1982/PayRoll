using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Organizations;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Domain.ValueObjects;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Seeder;

public sealed class PayrollSeeder
{
    private const string SeedUser = "seed";
    private readonly PayrollDbContext _context;

    public PayrollSeeder(PayrollDbContext context)
    {
        _context = context;
    }

    public void SeedMasterData()
    {
        var company = EnsureCompany("SLPAY", "Sri Lanka Payroll Demo (EPF: EPF-REG-0001, ETF: ETF-REG-0001)");
        var hqBranch = EnsureBranch("HQ", "Head Office", company.Id);
        var kdyBranch = EnsureBranch("KDY", "Kandy Branch", company.Id);

        EnsureCostCenter("CORP", "Corporate Services", null, company.Id);
        EnsureCostCenter("HQ-OPS", "Head Office Operations", hqBranch.Id, null);
        EnsureCostCenter("KDY-OPS", "Kandy Operations", kdyBranch.Id, null);

        EnsurePayrollSettings();
        EnsureOvertimeRule();

        EnsureAllowanceType("BASIC", "Basic Salary", CalculationBasis.FixedAmount, true, true, true);
        EnsureAllowanceType("ALW", "General Allowance", CalculationBasis.FixedAmount, true, true, true);
        EnsureAllowanceType("OT", "Overtime", CalculationBasis.PerHour, true, true, true);

        EnsureDeductionType("EPF_EE", "Employee EPF", CalculationBasis.PercentageOfBasic, true, false);
        EnsureDeductionType("EPF_ER", "Employer EPF", CalculationBasis.PercentageOfBasic, true, false);
        EnsureDeductionType("ETF_ER", "Employer ETF", CalculationBasis.PercentageOfBasic, true, false);
        EnsureDeductionType("PAYE", "PAYE Tax", CalculationBasis.FixedAmount, false, true);
        EnsureDeductionType("NOPAY", "No Pay", CalculationBasis.PerDay, true, false);

        EnsureEpfEtfRuleSet();
        EnsureTaxRuleSet();
    }

    public void SeedScenarioData()
    {
        SeedMasterData();

        var company = _context.Companies.Single(c => c.Code == "SLPAY");
        var hqBranch = _context.Branches.Single(b => b.Code == "HQ");
        var kdyBranch = _context.Branches.Single(b => b.Code == "KDY");
        var corpCostCenter = _context.CostCenters.Single(cc => cc.Code == "CORP");
        var hqCostCenter = _context.CostCenters.Single(cc => cc.Code == "HQ-OPS");
        var kdyCostCenter = _context.CostCenters.Single(cc => cc.Code == "KDY-OPS");

        var transportAllowance = EnsureAllowanceType("ALW_TRAN", "Transport Allowance", CalculationBasis.FixedAmount, true, true, true);
        var nonContribAllowance = EnsureAllowanceType("ALW_NC", "Non-Contributable Allowance", CalculationBasis.FixedAmount, true, false, false);
        var tempDeduction = EnsureDeductionType("DED_TEMP", "Temporary Deduction", CalculationBasis.FixedAmount, true, false);

        var salariedEmployee = EnsureEmployee(
            "EMP-SAL",
            "Sahan",
            "Perera",
            120_000m,
            company.Id,
            hqBranch.Id,
            hqCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: "1234567890");

        var hourlyEmployee = EnsureEmployee(
            "EMP-HR",
            "Nimali",
            "Fernando",
            45_000m,
            company.Id,
            kdyBranch.Id,
            kdyCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "032",
            bankAccountNumber: "5566778899");

        var inactiveEmployee = EnsureEmployee(
            "EMP-INACTIVE",
            "Roshan",
            "Silva",
            65_000m,
            company.Id,
            hqBranch.Id,
            corpCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: "4455667788");
        EnsureEmployeeInactive(inactiveEmployee);

        var missingBankEmployee = EnsureEmployee(
            "EMP-NOBANK",
            "Iresha",
            "Jayasinghe",
            55_000m,
            company.Id,
            hqBranch.Id,
            hqCostCenter.Id,
            bankName: null,
            bankCode: null,
            branchCode: null,
            bankAccountNumber: null);

        var taxBoundaryEmployee = EnsureEmployee(
            "EMP-TAX",
            "Chathura",
            "Wijesinghe",
            210_000m,
            company.Id,
            hqBranch.Id,
            hqCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: "9988776655");

        var epfScenarioEmployee = EnsureEmployee(
            "EMP-EPF",
            "Lakshmi",
            "Gunasekara",
            90_000m,
            company.Id,
            kdyBranch.Id,
            kdyCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "032",
            bankAccountNumber: "2233445566");

        EnsureRecurringRule(
            "RR-ALW-MON",
            "Monthly Allowance",
            RecurringRuleType.Allowance,
            salariedEmployee.Id,
            new DateOnly(2025, 4, 1),
            null,
            5_000m,
            true,
            true,
            true,
            true);

        EnsureRecurringRule(
            "RR-DED-TEMP",
            "Temporary Deduction",
            RecurringRuleType.Deduction,
            salariedEmployee.Id,
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 6, 30),
            1_500m,
            false,
            false,
            false,
            true);

        EnsureRecurringRule(
            "RR-INACTIVE",
            "Inactive Test Rule",
            RecurringRuleType.Allowance,
            hourlyEmployee.Id,
            new DateOnly(2025, 4, 1),
            null,
            2_000m,
            true,
            true,
            true,
            false);

        EnsureEmployeeRecurringPayItem(
            salariedEmployee.Id,
            PayItemKind.Allowance,
            transportAllowance.Id,
            null,
            4_000m,
            new DateOnly(2025, 4, 15),
            null,
            true);

        EnsureEmployeeRecurringPayItem(
            salariedEmployee.Id,
            PayItemKind.Deduction,
            null,
            tempDeduction.Id,
            1_200m,
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 6, 30),
            true);

        EnsureEmployeeRecurringPayItem(
            epfScenarioEmployee.Id,
            PayItemKind.Allowance,
            nonContribAllowance.Id,
            null,
            3_500m,
            new DateOnly(2025, 4, 1),
            null,
            true);

        EnsureAttendanceAbsence(hourlyEmployee.Id, new DateOnly(2025, 4, 8));
        EnsureAttendancePartial(hourlyEmployee.Id, new DateOnly(2025, 4, 18), 4m);

        EnsureOvertimeEntry(hourlyEmployee.Id, new DateOnly(2025, 4, 10), 3.5, OvertimeType.Weekday, OvertimeStatus.Approved);
        EnsureOvertimeEntry(hourlyEmployee.Id, new DateOnly(2025, 4, 12), 4, OvertimeType.Weekend, OvertimeStatus.Approved);
        EnsureOvertimeEntry(hourlyEmployee.Id, new DateOnly(2025, 4, 20), 2.5, OvertimeType.Weekday, OvertimeStatus.Pending);

        var draftPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = "PR-APR25-DRAFT",
            Name = "April 2025 Draft",
            Reference = "APR-2025-DRAFT",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Draft,
            IsLocked = false,
            CompanyId = company.Id,
            BranchId = hqBranch.Id,
            CostCenterId = hqCostCenter.Id
        });

        var preparedPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = "PR-APR25-PREP",
            Name = "April 2025 Prepared",
            Reference = "APR-2025-PREP",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Prepared,
            IsLocked = false,
            CompanyId = company.Id,
            BranchId = hqBranch.Id,
            CostCenterId = hqCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User"
        });
        EnsurePayRunApproval(preparedPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");

        var approvedPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = "PR-APR25-APPR",
            Name = "April 2025 Approved",
            Reference = "APR-2025-APPR",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Approved,
            IsLocked = false,
            CompanyId = company.Id,
            BranchId = hqBranch.Id,
            CostCenterId = hqCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User",
            ApprovedAt = new DateTime(2025, 4, 29, 10, 0, 0, DateTimeKind.Utc),
            ApprovedByUserName = "Seeder Approver"
        });
        EnsurePayRunApproval(approvedPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");
        EnsurePayRunApproval(approvedPayRun.Id, PayRunStatus.Prepared, PayRunStatus.Approved, "Approved by Seeder");

        var lockedPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = "PR-APR25-LOCK",
            Name = "April 2025 Locked (Valid Bank)",
            Reference = "APR-2025-LOCK",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Locked,
            IsLocked = true,
            CompanyId = company.Id,
            BranchId = hqBranch.Id,
            CostCenterId = hqCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User",
            ApprovedAt = new DateTime(2025, 4, 29, 10, 0, 0, DateTimeKind.Utc),
            ApprovedByUserName = "Seeder Approver",
            LockedAt = new DateTime(2025, 4, 30, 12, 0, 0, DateTimeKind.Utc),
            LockedByUserName = "Seeder Locker"
        });
        EnsurePayRunApproval(lockedPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");
        EnsurePayRunApproval(lockedPayRun.Id, PayRunStatus.Prepared, PayRunStatus.Approved, "Approved by Seeder");
        EnsurePayRunApproval(lockedPayRun.Id, PayRunStatus.Approved, PayRunStatus.Locked, "Locked by Seeder");

        var lockedInvalidPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = "PR-APR25-LOCK-BAD",
            Name = "April 2025 Locked (Invalid Bank)",
            Reference = "APR-2025-LOCK-BAD",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Locked,
            IsLocked = true,
            CompanyId = company.Id,
            BranchId = hqBranch.Id,
            CostCenterId = hqCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User",
            ApprovedAt = new DateTime(2025, 4, 29, 10, 0, 0, DateTimeKind.Utc),
            ApprovedByUserName = "Seeder Approver",
            LockedAt = new DateTime(2025, 4, 30, 12, 30, 0, DateTimeKind.Utc),
            LockedByUserName = "Seeder Locker"
        });
        EnsurePayRunApproval(lockedInvalidPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");
        EnsurePayRunApproval(lockedInvalidPayRun.Id, PayRunStatus.Prepared, PayRunStatus.Approved, "Approved by Seeder");
        EnsurePayRunApproval(lockedInvalidPayRun.Id, PayRunStatus.Approved, PayRunStatus.Locked, "Locked by Seeder");

        EnsurePaySlip(lockedPayRun.Id, salariedEmployee.Id, salariedEmployee.BaseSalary, 8_000m, 2_500m, 125_500m, 9_600m, 14_400m, 3_600m, 6_000m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = salariedEmployee.BaseSalary,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = transportAllowance.Code,
                    Description = transportAllowance.Name,
                    Amount = 8_000m,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                }
            },
            new List<DeductionLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "EPF_EE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 9_600m,
                    IsPreTax = true,
                    IsPostTax = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "PAYE",
                    Description = "PAYE",
                    Source = "Tax",
                    Amount = 6_000m,
                    IsPreTax = false,
                    IsPostTax = true
                }
            });

        EnsurePaySlip(lockedPayRun.Id, hourlyEmployee.Id, hourlyEmployee.BaseSalary, 4_500m, 1_200m, 48_300m, 3_600m, 5_400m, 1_350m, 1_200m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = hourlyEmployee.BaseSalary,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "OT",
                    Description = "Overtime",
                    Amount = 4_500m,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                }
            },
            new List<DeductionLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "EPF_EE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 3_600m,
                    IsPreTax = true,
                    IsPostTax = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "NOPAY",
                    Description = "No Pay",
                    Source = "Attendance",
                    Amount = 1_200m,
                    IsPreTax = true,
                    IsPostTax = false
                }
            });

        EnsurePaySlip(lockedPayRun.Id, taxBoundaryEmployee.Id, taxBoundaryEmployee.BaseSalary, 10_000m, 25_000m, 195_000m, 16_800m, 25_200m, 6_300m, 25_000m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = taxBoundaryEmployee.BaseSalary,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "ALW",
                    Description = "General Allowance",
                    Amount = 10_000m,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                }
            },
            new List<DeductionLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "EPF_EE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 16_800m,
                    IsPreTax = true,
                    IsPostTax = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "PAYE",
                    Description = "PAYE",
                    Source = "Tax",
                    Amount = 25_000m,
                    IsPreTax = false,
                    IsPostTax = true
                }
            });

        EnsurePaySlip(lockedPayRun.Id, epfScenarioEmployee.Id, epfScenarioEmployee.BaseSalary, 3_500m, 1_500m, 92_000m, 7_200m, 10_800m, 2_700m, 1_500m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = epfScenarioEmployee.BaseSalary,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = nonContribAllowance.Code,
                    Description = nonContribAllowance.Name,
                    Amount = 3_500m,
                    IsEpfApplicable = false,
                    IsEtfApplicable = false,
                    IsTaxable = true
                }
            },
            new List<DeductionLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "EPF_EE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 7_200m,
                    IsPreTax = true,
                    IsPostTax = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "PAYE",
                    Description = "PAYE",
                    Source = "Tax",
                    Amount = 1_500m,
                    IsPreTax = false,
                    IsPostTax = true
                }
            });

        EnsurePaySlip(lockedInvalidPayRun.Id, missingBankEmployee.Id, missingBankEmployee.BaseSalary, 2_500m, 500m, 57_000m, 4_400m, 6_600m, 1_650m, 500m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = missingBankEmployee.BaseSalary,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "ALW",
                    Description = "General Allowance",
                    Amount = 2_500m,
                    IsEpfApplicable = true,
                    IsEtfApplicable = true,
                    IsTaxable = true
                }
            },
            new List<DeductionLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "EPF_EE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 4_400m,
                    IsPreTax = true,
                    IsPostTax = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "PAYE",
                    Description = "PAYE",
                    Source = "Tax",
                    Amount = 500m,
                    IsPreTax = false,
                    IsPostTax = true
                }
            });
    }

    public void ResetAndSeedAll()
    {
        ResetData();
        SeedMasterData();
        SeedScenarioData();
    }

    private void ResetData()
    {
        _context.PayRunApprovals.RemoveRange(_context.PayRunApprovals);
        _context.PaySlips.RemoveRange(_context.PaySlips);
        _context.PayRuns.RemoveRange(_context.PayRuns);
        _context.OTEntries.RemoveRange(_context.OTEntries);
        _context.AttendanceRecords.RemoveRange(_context.AttendanceRecords);
        _context.LeaveRequests.RemoveRange(_context.LeaveRequests);
        _context.EmployeeRecurringPayItems.RemoveRange(_context.EmployeeRecurringPayItems);
        _context.EmployeePayItems.RemoveRange(_context.EmployeePayItems);
        _context.RecurringRules.RemoveRange(_context.RecurringRules);
        _context.Loans.RemoveRange(_context.Loans);
        _context.LoanRepayments.RemoveRange(_context.LoanRepayments);
        _context.Employees.RemoveRange(_context.Employees);
        _context.OTRules.RemoveRange(_context.OTRules);
        _context.PayrollSettings.RemoveRange(_context.PayrollSettings);
        _context.AllowanceTypes.RemoveRange(_context.AllowanceTypes);
        _context.DeductionTypes.RemoveRange(_context.DeductionTypes);
        _context.EpfEtfRuleSets.RemoveRange(_context.EpfEtfRuleSets);
        _context.TaxReliefs.RemoveRange(_context.TaxReliefs);
        _context.TaxSlabs.RemoveRange(_context.TaxSlabs);
        _context.TaxRuleSets.RemoveRange(_context.TaxRuleSets);
        _context.CostCenters.RemoveRange(_context.CostCenters);
        _context.Branches.RemoveRange(_context.Branches);
        _context.Companies.RemoveRange(_context.Companies);

        _context.SaveChanges();
    }

    private Company EnsureCompany(string code, string name)
    {
        var company = _context.Companies.SingleOrDefault(c => c.Code == code);
        if (company is not null)
        {
            return company;
        }

        company = new Company(code, name)
        {
            CreatedBy = SeedUser
        };

        _context.Companies.Add(company);
        _context.SaveChanges();

        return company;
    }

    private Branch EnsureBranch(string code, string name, Guid companyId)
    {
        var branch = _context.Branches.SingleOrDefault(b => b.Code == code && b.CompanyId == companyId);
        if (branch is not null)
        {
            return branch;
        }

        branch = new Branch(code, name, companyId)
        {
            CreatedBy = SeedUser
        };

        _context.Branches.Add(branch);
        _context.SaveChanges();

        return branch;
    }

    private void EnsureCostCenter(string code, string name, Guid? branchId, Guid? companyId)
    {
        if (_context.CostCenters.Any(cc => cc.Code == code))
        {
            return;
        }

        var costCenter = new CostCenter(code, name, branchId, companyId)
        {
            CreatedBy = SeedUser
        };

        _context.CostCenters.Add(costCenter);
        _context.SaveChanges();
    }

    private void EnsurePayrollSettings()
    {
        if (_context.PayrollSettings.Any())
        {
            return;
        }

        _context.PayrollSettings.Add(new PayrollSettings
        {
            WorkingDaysPerMonth = 22,
            WorkingHoursPerDay = 8,
            NoPayCalculationBasis = CalculationBasis.PerDay,
            AttendanceHalfDayHours = 4,
            WeekdayOvertimeMultiplier = 1.5m,
            WeekendOvertimeMultiplier = 2m,
            HolidayOvertimeMultiplier = 2.5m,
            OvertimeRoundingMinutes = 15,
            OvertimeDailyCapHours = 4,
            OvertimePayRunCapHours = 40,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsureOvertimeRule()
    {
        if (_context.OTRules.Any())
        {
            return;
        }

        _context.OTRules.Add(new OTRule
        {
            Name = "Sri Lanka Default OT",
            WeekdayMultiplier = 1.5m,
            WeekendMultiplier = 2m,
            HolidayMultiplier = 2.5m,
            RoundingMinutes = 15,
            DailyCapHours = 4,
            PayRunCapHours = 40,
            AppliesOnWeekend = true,
            AppliesOnHoliday = true,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private AllowanceType EnsureAllowanceType(string code, string name, CalculationBasis basis, bool isTaxable, bool isEpfApplicable, bool isEtfApplicable)
    {
        var existing = _context.AllowanceTypes.SingleOrDefault(a => a.Code == code);
        if (existing is not null)
        {
            return existing;
        }

        var allowance = new AllowanceType
        {
            Code = code,
            Name = name,
            Basis = basis,
            IsTaxable = isTaxable,
            IsEpfApplicable = isEpfApplicable,
            IsEtfApplicable = isEtfApplicable,
            IsActive = true,
            CreatedBy = SeedUser
        };

        _context.AllowanceTypes.Add(allowance);
        _context.SaveChanges();

        return allowance;
    }

    private DeductionType EnsureDeductionType(string code, string name, CalculationBasis basis, bool isPreTax, bool isPostTax)
    {
        var existing = _context.DeductionTypes.SingleOrDefault(d => d.Code == code);
        if (existing is not null)
        {
            return existing;
        }

        var deduction = new DeductionType
        {
            Code = code,
            Name = name,
            Basis = basis,
            IsPreTax = isPreTax,
            IsPostTax = isPostTax,
            IsActive = true,
            CreatedBy = SeedUser
        };

        _context.DeductionTypes.Add(deduction);
        _context.SaveChanges();

        return deduction;
    }

    private void EnsureEpfEtfRuleSet()
    {
        if (_context.EpfEtfRuleSets.Any(r => r.IsDefault))
        {
            return;
        }

        _context.EpfEtfRuleSets.Add(new EpfEtfRuleSet
        {
            Name = "Sri Lanka Default EPF/ETF (Sample)",
            EffectiveFrom = new DateOnly(2020, 1, 1),
            EffectiveTo = null,
            EmployeeEpfRate = 8m,
            EmployerEpfRate = 12m,
            EmployerEtfRate = 3m,
            MinimumWageForEpf = null,
            MaximumEarningForEpf = null,
            MaximumEarningForEtf = null,
            IsDefault = true,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsureTaxRuleSet()
    {
        if (_context.TaxRuleSets.Any(r => r.Name == "Sri Lanka PAYE (Sample)"))
        {
            return;
        }

        var ruleSet = new TaxRuleSet
        {
            Name = "Sri Lanka PAYE (Sample)",
            YearOfAssessment = 2025,
            EffectiveFrom = new DateOnly(2025, 4, 1),
            EffectiveTo = null,
            IsDefault = !_context.TaxRuleSets.Any(r => r.IsDefault),
            IsActive = true,
            CreatedBy = SeedUser
        };

        var slabs = new List<TaxSlab>
        {
            new()
            {
                TaxRuleSet = ruleSet,
                FromAmount = 0m,
                ToAmount = 100_000m,
                RatePercent = 0m,
                Order = 1,
                CreatedBy = SeedUser
            },
            new()
            {
                TaxRuleSet = ruleSet,
                FromAmount = 100_000m,
                ToAmount = 141_667m,
                RatePercent = 6m,
                Order = 2,
                CreatedBy = SeedUser
            },
            new()
            {
                TaxRuleSet = ruleSet,
                FromAmount = 141_667m,
                ToAmount = 183_333m,
                RatePercent = 12m,
                Order = 3,
                CreatedBy = SeedUser
            },
            new()
            {
                TaxRuleSet = ruleSet,
                FromAmount = 183_333m,
                ToAmount = null,
                RatePercent = 18m,
                Order = 4,
                CreatedBy = SeedUser
            }
        };

        _context.TaxRuleSets.Add(ruleSet);
        _context.TaxSlabs.AddRange(slabs);
        _context.SaveChanges();
    }

    private Employee EnsureEmployee(
        string code,
        string firstName,
        string lastName,
        decimal baseSalary,
        Guid companyId,
        Guid branchId,
        Guid costCenterId,
        string? bankName,
        string? bankCode,
        string? branchCode,
        string? bankAccountNumber)
    {
        var existing = _context.Employees.SingleOrDefault(e => e.EmployeeCode == code);
        if (existing is not null)
        {
            return existing;
        }

        var employee = Employee.Create(
            code,
            firstName,
            lastName,
            $"{code}-NIC",
            new DateTime(1990, 1, 1),
            Gender.Male,
            MaritalStatus.Single,
            new DateTime(2020, 1, 1),
            baseSalary,
            companyId,
            branchId,
            costCenterId,
            null,
            null,
            null,
            null,
            SeedUser,
            bankName,
            bankCode,
            branchCode,
            bankAccountNumber);

        _context.Employees.Add(employee);
        _context.SaveChanges();

        return employee;
    }

    private void EnsureEmployeeInactive(Employee employee)
    {
        if (!employee.IsActive)
        {
            return;
        }

        employee.SoftDelete(SeedUser);
        _context.SaveChanges();
    }

    private void EnsureRecurringRule(
        string code,
        string name,
        RecurringRuleType type,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate,
        decimal amount,
        bool isTaxable,
        bool isEpfApplicable,
        bool isEtfApplicable,
        bool isActive)
    {
        if (_context.RecurringRules.Any(r => r.Code == code))
        {
            return;
        }

        _context.RecurringRules.Add(new RecurringRule
        {
            Code = code,
            Name = name,
            RuleType = type,
            Frequency = PayPeriodType.Monthly,
            StartDate = startDate,
            EndDate = endDate,
            Amount = amount,
            EmployeeId = employeeId,
            IsTaxable = isTaxable,
            IsEpfApplicable = isEpfApplicable,
            IsEtfApplicable = isEtfApplicable,
            IsActive = isActive,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsureEmployeeRecurringPayItem(
        Guid employeeId,
        PayItemKind payItemKind,
        Guid? allowanceTypeId,
        Guid? deductionTypeId,
        decimal amount,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isActive)
    {
        var exists = _context.EmployeeRecurringPayItems.Any(pi =>
            pi.EmployeeId == employeeId
            && pi.PayItemKind == payItemKind
            && pi.AllowanceTypeId == allowanceTypeId
            && pi.DeductionTypeId == deductionTypeId
            && pi.EffectiveFrom == effectiveFrom);

        if (exists)
        {
            return;
        }

        _context.EmployeeRecurringPayItems.Add(new EmployeeRecurringPayItem
        {
            EmployeeId = employeeId,
            PayItemKind = payItemKind,
            AllowanceTypeId = allowanceTypeId,
            DeductionTypeId = deductionTypeId,
            Amount = amount,
            Percentage = null,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = isActive,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsureAttendanceAbsence(Guid employeeId, DateOnly date)
    {
        if (_context.AttendanceRecords.Any(a => a.EmployeeId == employeeId && a.Period.Start == date && a.Period.End == date))
        {
            return;
        }

        _context.AttendanceRecords.Add(new AttendanceRecord
        {
            EmployeeId = employeeId,
            Period = new DateRange(date, date),
            HoursWorked = 0,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsureAttendancePartial(Guid employeeId, DateOnly date, decimal hoursWorked)
    {
        if (_context.AttendanceRecords.Any(a => a.EmployeeId == employeeId && a.Period.Start == date && a.Period.End == date))
        {
            return;
        }

        _context.AttendanceRecords.Add(new AttendanceRecord
        {
            EmployeeId = employeeId,
            Period = new DateRange(date, date),
            HoursWorked = hoursWorked,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsureOvertimeEntry(Guid employeeId, DateOnly date, double hours, OvertimeType type, OvertimeStatus status)
    {
        if (_context.OTEntries.Any(o => o.EmployeeId == employeeId && o.Date == date && o.Type == type))
        {
            return;
        }

        _context.OTEntries.Add(new OTEntry
        {
            EmployeeId = employeeId,
            Date = date,
            Hours = hours,
            Type = type,
            Status = status,
            ApprovedAt = status == OvertimeStatus.Approved ? DateTimeOffset.UtcNow : null,
            IsLockedForPayroll = false,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private PayRun EnsurePayRun(PayRunSeed seed)
    {
        var existing = _context.PayRuns.SingleOrDefault(pr => pr.Code == seed.Code);
        if (existing is not null)
        {
            return existing;
        }

        var payRun = new PayRun
        {
            Code = seed.Code,
            Name = seed.Name,
            Reference = seed.Reference,
            PeriodType = PayPeriodType.Monthly,
            PeriodStart = seed.PeriodStart,
            PeriodEnd = seed.PeriodEnd,
            PayDate = seed.PayDate,
            Status = seed.Status,
            IsLocked = seed.IsLocked,
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CostCenterId = seed.CostCenterId,
            PreparedAt = seed.PreparedAt,
            PreparedByUserName = seed.PreparedByUserName,
            ApprovedAt = seed.ApprovedAt,
            ApprovedByUserName = seed.ApprovedByUserName,
            LockedAt = seed.LockedAt,
            LockedByUserName = seed.LockedByUserName,
            CreatedBy = SeedUser
        };

        _context.PayRuns.Add(payRun);
        _context.SaveChanges();

        return payRun;
    }

    private void EnsurePayRunApproval(Guid payRunId, PayRunStatus fromStatus, PayRunStatus toStatus, string comment)
    {
        if (_context.PayRunApprovals.Any(a => a.PayRunId == payRunId && a.FromStatus == fromStatus && a.ToStatus == toStatus))
        {
            return;
        }

        _context.PayRunApprovals.Add(new PayRunApproval
        {
            PayRunId = payRunId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActorUserName = "Seeder",
            Comment = comment,
            ActionedAt = DateTime.UtcNow,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
    }

    private void EnsurePaySlip(
        Guid payRunId,
        Guid employeeId,
        decimal basicSalary,
        decimal totalEarnings,
        decimal totalDeductions,
        decimal netPay,
        decimal employeeEpf,
        decimal employerEpf,
        decimal employerEtf,
        decimal payeTax,
        List<EarningLine> earnings,
        List<DeductionLine> deductions)
    {
        if (_context.PaySlips.Any(ps => ps.PayRunId == payRunId && ps.EmployeeId == employeeId))
        {
            return;
        }

        var paySlip = new PaySlip
        {
            PayRunId = payRunId,
            EmployeeId = employeeId,
            BasicSalary = basicSalary,
            TotalEarnings = totalEarnings,
            TotalDeductions = totalDeductions,
            NetPay = netPay,
            EmployeeEpf = employeeEpf,
            EmployerEpf = employerEpf,
            EmployerEtf = employerEtf,
            PayeTax = payeTax,
            Earnings = earnings,
            Deductions = deductions,
            CreatedBy = SeedUser
        };

        _context.PaySlips.Add(paySlip);
        _context.SaveChanges();
    }

    private sealed class PayRunSeed
    {
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
        public DateTime PeriodStart { get; init; }
        public DateTime PeriodEnd { get; init; }
        public DateTime PayDate { get; init; }
        public PayRunStatus Status { get; init; }
        public bool IsLocked { get; init; }
        public Guid? CompanyId { get; init; }
        public Guid? BranchId { get; init; }
        public Guid? CostCenterId { get; init; }
        public DateTime? PreparedAt { get; init; }
        public string? PreparedByUserName { get; init; }
        public DateTime? ApprovedAt { get; init; }
        public string? ApprovedByUserName { get; init; }
        public DateTime? LockedAt { get; init; }
        public string? LockedByUserName { get; init; }
    }
}
