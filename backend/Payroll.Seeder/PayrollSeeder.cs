using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Attendance;
using Payroll.Domain.Leave;
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
    private const string DemoPrefix = "DEMO_";
    private const string DemoCompanyCode = "DEMO_LANKA";
    private readonly PayrollDbContext _context;

    public PayrollSeeder(PayrollDbContext context)
    {
        _context = context;
    }

    public void SeedMasterData()
    {
        var company = EnsureCompany(DemoCompanyCode, "Demo Lanka (Pvt) Ltd");
        var colomboBranch = EnsureBranch("COL", "Colombo", company.Id);
        var kandyBranch = EnsureBranch("KDY", "Kandy", company.Id);

        EnsureCostCenter("OPS", "Operations", null, company.Id);
        EnsureCostCenter("SALES", "Sales", null, company.Id);
        EnsureCostCenter("FIN", "Finance", null, company.Id);

        EnsurePayrollSettings();
        EnsureOvertimeRule();

        EnsureAllowanceType("BASIC", "Basic Salary", CalculationBasis.FixedAmount, true, true, true);
        EnsureAllowanceType("ALLOW_TRANSPORT", "Transport Allowance", CalculationBasis.FixedAmount, true, true, true);
        EnsureAllowanceType("OT", "Overtime", CalculationBasis.PerHour, true, true, true);

        EnsureDeductionType("EPF_EMPLOYEE", "Employee EPF", CalculationBasis.PercentageOfBasic, true, false);
        EnsureDeductionType("EPF_EMPLOYER", "Employer EPF", CalculationBasis.PercentageOfBasic, false, false);
        EnsureDeductionType("ETF_EMPLOYER", "Employer ETF", CalculationBasis.PercentageOfBasic, false, false);
        EnsureDeductionType("PAYE", "PAYE", CalculationBasis.FixedAmount, false, true);
        EnsureDeductionType("DED_NO_PAY", "No Pay", CalculationBasis.PerDay, true, false);

        EnsureEpfEtfRuleSet();
        EnsureTaxRuleSet();
    }

    public void SeedScenarioData()
    {
        SeedMasterData();

        var company = _context.Companies.Single(c => c.Code == DemoCompanyCode);
        var colomboBranch = _context.Branches.Single(b => b.Code == "COL");
        var kandyBranch = _context.Branches.Single(b => b.Code == "KDY");
        var opsCostCenter = _context.CostCenters.Single(cc => cc.Code == "OPS");
        var salesCostCenter = _context.CostCenters.Single(cc => cc.Code == "SALES");
        var financeCostCenter = _context.CostCenters.Single(cc => cc.Code == "FIN");

        var transportAllowance = EnsureAllowanceType("ALLOW_TRANSPORT", "Transport Allowance", CalculationBasis.FixedAmount, true, true, true);
        var nonContribAllowance = EnsureAllowanceType($"{DemoPrefix}ALLOW_NC", "Demo Non-Contributable Allowance", CalculationBasis.FixedAmount, true, false, false);
        var tempDeduction = EnsureDeductionType($"{DemoPrefix}DED_TEMP", "Demo Temporary Deduction", CalculationBasis.FixedAmount, true, false);

        var salariedEmployee = EnsureEmployee(
            $"{DemoPrefix}EMP_1",
            "Sahan",
            "Perera",
            "901234567V",
            120_000m,
            company.Id,
            colomboBranch.Id,
            opsCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: "1234567890");

        var missingBankEmployee = EnsureEmployee(
            $"{DemoPrefix}EMP_2",
            "Nimali",
            "Fernando",
            "925678901V",
            95_000m,
            company.Id,
            colomboBranch.Id,
            opsCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: null);

        var lowerSalaryEmployee = EnsureEmployee(
            $"{DemoPrefix}EMP_3",
            "Chathura",
            "Wijesinghe",
            "880112233V",
            45_000m,
            company.Id,
            colomboBranch.Id,
            opsCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: "9988776655");

        var inactiveEmployee = EnsureEmployee(
            $"{DemoPrefix}EMP_4",
            "Roshan",
            "Silva",
            "831234567V",
            65_000m,
            company.Id,
            colomboBranch.Id,
            salesCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "001",
            bankAccountNumber: "4455667788");
        EnsureEmployeeInactive(inactiveEmployee);

        var branchEmployee = EnsureEmployee(
            $"{DemoPrefix}EMP_5",
            "Lakshmi",
            "Gunasekara",
            "947654321V",
            90_000m,
            company.Id,
            kandyBranch.Id,
            financeCostCenter.Id,
            bankName: "HNB",
            bankCode: "7083",
            branchCode: "032",
            bankAccountNumber: "2233445566");

        EnsureRecurringRule(
            $"{DemoPrefix}RR_ALLOW",
            "Demo Monthly Allowance",
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
            $"{DemoPrefix}RR_DED_TEMP",
            "Demo Temporary Deduction",
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
            $"{DemoPrefix}RR_INACTIVE",
            "Demo Inactive Test Rule",
            RecurringRuleType.Allowance,
            lowerSalaryEmployee.Id,
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
            branchEmployee.Id,
            PayItemKind.Allowance,
            nonContribAllowance.Id,
            null,
            3_500m,
            new DateOnly(2025, 4, 1),
            null,
            true);

        EnsureAttendanceAbsence(lowerSalaryEmployee.Id, new DateOnly(2025, 4, 8));
        EnsureAttendancePartial(lowerSalaryEmployee.Id, new DateOnly(2025, 4, 18), 4m);

        EnsureOvertimeEntry(lowerSalaryEmployee.Id, new DateOnly(2025, 4, 10), 3.5, OvertimeType.Weekday, OvertimeStatus.Approved);
        EnsureOvertimeEntry(lowerSalaryEmployee.Id, new DateOnly(2025, 4, 12), 4, OvertimeType.Weekend, OvertimeStatus.Approved);
        EnsureOvertimeEntry(lowerSalaryEmployee.Id, new DateOnly(2025, 4, 20), 2.5, OvertimeType.Weekday, OvertimeStatus.Pending);

        EnsureLeaveRequest(salariedEmployee.Id, new DateOnly(2025, 4, 22), new DateOnly(2025, 4, 22), 1, "Approved demo leave");

        EnsurePayRun(new PayRunSeed
        {
            Code = $"{DemoPrefix}PR_2025_04_DRAFT",
            Name = "April 2025 Draft",
            Reference = "APR-2025-DRAFT",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Draft,
            IsLocked = false,
            CompanyId = company.Id,
            BranchId = colomboBranch.Id,
            CostCenterId = opsCostCenter.Id
        });

        var preparedPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = $"{DemoPrefix}PR_2025_04_PREP",
            Name = "April 2025 Prepared",
            Reference = "APR-2025-PREP",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Prepared,
            IsLocked = false,
            CompanyId = company.Id,
            BranchId = colomboBranch.Id,
            CostCenterId = opsCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User"
        });
        EnsurePayRunStatusHistory(preparedPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");

        var approvedPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = $"{DemoPrefix}PR_2025_04_APPR",
            Name = "April 2025 Approved",
            Reference = "APR-2025-APPR",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Approved,
            IsLocked = false,
            CompanyId = company.Id,
            BranchId = colomboBranch.Id,
            CostCenterId = opsCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User",
            ApprovedAt = new DateTime(2025, 4, 29, 10, 0, 0, DateTimeKind.Utc),
            ApprovedByUserName = "Seeder Approver"
        });
        EnsurePayRunStatusHistory(approvedPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");
        EnsurePayRunStatusHistory(approvedPayRun.Id, PayRunStatus.Prepared, PayRunStatus.Approved, "Approved by Seeder");

        var lockedPayRun = EnsurePayRun(new PayRunSeed
        {
            Code = $"{DemoPrefix}PR_2025_04_LOCK",
            Name = "April 2025 Locked",
            Reference = "APR-2025-LOCK",
            PeriodStart = new DateTime(2025, 4, 1),
            PeriodEnd = new DateTime(2025, 4, 30),
            PayDate = new DateTime(2025, 4, 30),
            Status = PayRunStatus.Locked,
            IsLocked = true,
            CompanyId = company.Id,
            BranchId = colomboBranch.Id,
            CostCenterId = opsCostCenter.Id,
            PreparedAt = new DateTime(2025, 4, 28, 9, 0, 0, DateTimeKind.Utc),
            PreparedByUserName = "Seeder User",
            ApprovedAt = new DateTime(2025, 4, 29, 10, 0, 0, DateTimeKind.Utc),
            ApprovedByUserName = "Seeder Approver",
            LockedAt = new DateTime(2025, 4, 30, 12, 0, 0, DateTimeKind.Utc),
            LockedByUserName = "Seeder Locker",
            ExportedBank = "HNB"
        });
        EnsurePayRunStatusHistory(lockedPayRun.Id, PayRunStatus.Draft, PayRunStatus.Prepared, "Prepared by Seeder");
        EnsurePayRunStatusHistory(lockedPayRun.Id, PayRunStatus.Prepared, PayRunStatus.Approved, "Approved by Seeder");
        EnsurePayRunStatusHistory(lockedPayRun.Id, PayRunStatus.Approved, PayRunStatus.Locked, "Locked by Seeder");

        EnsurePaySlip(lockedPayRun.Id, salariedEmployee.Id, salariedEmployee.BaseSalary, 128_000m, 15_600m, 112_400m, 9_600m, 14_400m, 3_600m, 6_000m,
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
                    Code = "EPF_EMPLOYEE",
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

        EnsurePaySlip(lockedPayRun.Id, lowerSalaryEmployee.Id, lowerSalaryEmployee.BaseSalary, 49_500m, 4_800m, 44_700m, 3_600m, 5_400m, 1_350m, 0m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = lowerSalaryEmployee.BaseSalary,
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
                    Code = "EPF_EMPLOYEE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 3_600m,
                    IsPreTax = true,
                    IsPostTax = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "DED_NO_PAY",
                    Description = "No Pay",
                    Source = "Attendance",
                    Amount = 1_200m,
                    IsPreTax = true,
                    IsPostTax = false
                }
            });

        EnsurePaySlip(lockedPayRun.Id, branchEmployee.Id, branchEmployee.BaseSalary, 93_500m, 8_700m, 84_800m, 7_200m, 10_800m, 2_700m, 1_500m,
            new List<EarningLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    Description = "Basic Salary",
                    Amount = branchEmployee.BaseSalary,
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
                    Code = "EPF_EMPLOYEE",
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

        EnsurePaySlip(lockedPayRun.Id, missingBankEmployee.Id, missingBankEmployee.BaseSalary, 97_500m, 8_100m, 89_400m, 7_600m, 11_400m, 2_850m, 500m,
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
                    Code = transportAllowance.Code,
                    Description = transportAllowance.Name,
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
                    Code = "EPF_EMPLOYEE",
                    Description = "Employee EPF",
                    Source = "Statutory",
                    Amount = 7_600m,
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
        ResetScenarioData();
        SeedScenarioData();
    }

    private void ResetScenarioData()
    {
        var scenarioEmployees = _context.Employees
            .Where(e => e.EmployeeCode.StartsWith(DemoPrefix))
            .ToList();
        var scenarioEmployeeIds = scenarioEmployees.Select(e => e.Id).ToList();
        var scenarioPayRuns = _context.PayRuns
            .Where(pr => pr.Code.StartsWith(DemoPrefix))
            .ToList();
        var scenarioPayRunIds = scenarioPayRuns.Select(pr => pr.Id).ToList();
        var scenarioLoans = _context.Loans
            .Where(l => scenarioEmployeeIds.Contains(l.EmployeeId))
            .ToList();
        var scenarioLoanIds = scenarioLoans.Select(l => l.Id).ToList();

        _context.PayRunStatusHistories.RemoveRange(_context.PayRunStatusHistories.Where(h => scenarioPayRunIds.Contains(h.PayRunId)));
        _context.PaySlips.RemoveRange(_context.PaySlips.Where(ps => scenarioPayRunIds.Contains(ps.PayRunId)));
        _context.PayRuns.RemoveRange(scenarioPayRuns);
        _context.OTEntries.RemoveRange(_context.OTEntries.Where(o => scenarioEmployeeIds.Contains(o.EmployeeId)));
        _context.AttendanceRecords.RemoveRange(_context.AttendanceRecords.Where(a => scenarioEmployeeIds.Contains(a.EmployeeId)));
        _context.LeaveRequests.RemoveRange(_context.LeaveRequests.Where(l => scenarioEmployeeIds.Contains(l.EmployeeId)));
        _context.EmployeeRecurringPayItems.RemoveRange(_context.EmployeeRecurringPayItems.Where(pi => scenarioEmployeeIds.Contains(pi.EmployeeId)));
        _context.EmployeePayItems.RemoveRange(_context.EmployeePayItems.Where(pi => scenarioEmployeeIds.Contains(pi.EmployeeId)));
        _context.RecurringRules.RemoveRange(_context.RecurringRules.Where(r => r.Code.StartsWith(DemoPrefix)));
        _context.LoanRepayments.RemoveRange(_context.LoanRepayments.Where(lr => scenarioLoanIds.Contains(lr.LoanId)));
        _context.Loans.RemoveRange(scenarioLoans);
        _context.Employees.RemoveRange(scenarioEmployees);
        _context.AllowanceTypes.RemoveRange(_context.AllowanceTypes.Where(a => a.Code.StartsWith(DemoPrefix)));
        _context.DeductionTypes.RemoveRange(_context.DeductionTypes.Where(d => d.Code.StartsWith(DemoPrefix)));

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
            WorkingDaysPerMonth = 26,
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
        string nicNumber,
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
            nicNumber,
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

    private void EnsureLeaveRequest(Guid employeeId, DateOnly startDate, DateOnly endDate, double totalDays, string reason)
    {
        if (_context.LeaveRequests.Any(l => l.EmployeeId == employeeId && l.StartDate == startDate && l.EndDate == endDate))
        {
            return;
        }

        _context.LeaveRequests.Add(new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveType = LeaveTypeCode.Annual,
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = totalDays,
            Reason = reason,
            Status = LeaveStatus.Approved,
            ApprovedById = null,
            RequestedAt = DateTimeOffset.UtcNow.AddDays(-10),
            ApprovedAt = DateTimeOffset.UtcNow.AddDays(-8),
            IsHalfDay = false,
            HalfDaySession = null,
            CreatedBy = SeedUser
        });

        _context.SaveChanges();
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
            ExportedBank = seed.ExportedBank,
            CreatedBy = SeedUser
        };

        _context.PayRuns.Add(payRun);
        _context.SaveChanges();

        return payRun;
    }

    private void EnsurePayRunStatusHistory(Guid payRunId, PayRunStatus fromStatus, PayRunStatus toStatus, string comment)
    {
        if (_context.PayRunStatusHistories.Any(a => a.PayRunId == payRunId && a.FromStatus == fromStatus && a.ToStatus == toStatus))
        {
            return;
        }

        var timestampUtc = DateTime.UtcNow;
        var previousHash = _context.PayRunStatusHistories
            .Where(h => h.PayRunId == payRunId)
            .OrderByDescending(h => h.TimestampUtc)
            .ThenByDescending(h => h.Id)
            .Select(h => h.Hash)
            .FirstOrDefault();
        var previousHashValue = previousHash ?? string.Empty;
        var hashPayload = string.Join('|', payRunId, fromStatus, toStatus, string.Empty, comment, timestampUtc.ToString("O"), previousHashValue);

        _context.PayRunStatusHistories.Add(new PayRunStatusHistory
        {
            PayRunId = payRunId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActorDisplayName = "Seeder",
            Comment = comment,
            TimestampUtc = timestampUtc,
            PreviousHash = previousHashValue,
            Hash = ComputeSha256(hashPayload),
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

    private static string ComputeSha256(string value)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha.ComputeHash(bytes);
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
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
        public string? ExportedBank { get; init; }
    }
}
