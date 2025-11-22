using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class PayrollService : IPayrollService
{
    private const int WorkingDaysPerMonth = 26; // TODO: move to configuration
    private const int WorkingHoursPerDay = 8; // TODO: move to configuration
    private const decimal WeekdayOvertimeMultiplier = 1.5m; // TODO: move to configuration
    private const decimal WeekendOvertimeMultiplier = 2.0m; // TODO: move to configuration
    private const decimal PublicHolidayOvertimeMultiplier = 2.0m; // TODO: move to configuration

    private readonly IPayrollDbContext _dbContext;
    private readonly IEpfEtfRuleSetService _epfEtfRuleSetService;
    private readonly ITaxRuleSetService _taxRuleSetService;

    public PayrollService(
        IPayrollDbContext dbContext,
        IEpfEtfRuleSetService epfEtfRuleSetService,
        ITaxRuleSetService taxRuleSetService)
    {
        _dbContext = dbContext;
        _epfEtfRuleSetService = epfEtfRuleSetService;
        _taxRuleSetService = taxRuleSetService;
    }

    public async Task<PayRunDto> CreatePayRunAsync(PayRunDto payRunDto, CancellationToken cancellationToken = default)
    {
        var payRun = new PayRun
        {
            Reference = string.IsNullOrWhiteSpace(payRunDto.Reference) ? $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}" : payRunDto.Reference,
            PeriodStart = payRunDto.PeriodStart,
            PeriodEnd = payRunDto.PeriodEnd,
            PayDate = DateOnly.FromDateTime(payRunDto.PayDate == default ? payRunDto.PeriodEnd : payRunDto.PayDate),
            Status = PayRunStatus.Draft,
            IsLocked = false
        };

        var employeeIds = payRunDto.EmployeeIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        if (!employeeIds.Any())
        {
            employeeIds = await _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
        }

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        payRun.Status = PayRunStatus.Calculated;

        await _dbContext.PayRuns.AddAsync(payRun, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(payRun);
    }

    public async Task<PayRunDto> RecalculatePayRunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status is PayRunStatus.Approved or PayRunStatus.Posted or PayRunStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot recalculate an approved, posted, or cancelled pay run.");
        }

        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).ToList();

        _dbContext.PaySlips.RemoveRange(payRun.PaySlips);
        payRun.PaySlips.Clear();

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        payRun.Status = PayRunStatus.Calculated;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(payRun);
    }

    public async Task ApprovePayRunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns.FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status is PayRunStatus.Posted or PayRunStatus.Cancelled)
        {
            throw new InvalidOperationException("Pay run cannot be approved from the current status.");
        }

        payRun.Status = PayRunStatus.Approved;

        if (payRun.Status == PayRunStatus.Posted)
        {
            payRun.IsLocked = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PayRunDto?> GetPayRunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        return payRun is null ? null : MapToDto(payRun);
    }

    public async Task<PaginatedResult<PayRunDto>> GetPayRunsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var query = _dbContext.PayRuns.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PayRunDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaySlipDto?> GetPaySlipAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paySlip = await _dbContext.PaySlips
            .Include(ps => ps.Earnings)
            .Include(ps => ps.Deductions)
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.Id == id, cancellationToken);

        return paySlip is null ? null : MapToDto(paySlip);
    }

    private async Task<List<PaySlip>> GeneratePaySlipsForPayRunAsync(PayRun payRun, List<Guid> employeeIds, CancellationToken ct)
    {
        var employees = await _dbContext.Employees
            .Where(e => employeeIds.Contains(e.Id) && e.IsActive)
            .ToListAsync(ct);

        var periodStart = DateOnly.FromDateTime(payRun.PeriodStart);
        var periodEnd = DateOnly.FromDateTime(payRun.PeriodEnd);

        var attendance = await _dbContext.AttendanceRecords
            .Where(a => employeeIds.Contains(a.EmployeeId)
                        && a.Period.StartDate <= periodEnd.ToDateTime(TimeOnly.MinValue)
                        && a.Period.EndDate >= periodStart.ToDateTime(TimeOnly.MinValue))
            .ToListAsync(ct);

        var overtime = await _dbContext.OvertimeRecords
            .Where(o => employeeIds.Contains(o.EmployeeId)
                        && o.Date >= periodStart
                        && o.Date <= periodEnd
                        && o.Status == OvertimeStatus.Approved
                        && !o.IsLockedForPayroll)
            .ToListAsync(ct);

        var loans = await _dbContext.Loans
            .Where(l => employeeIds.Contains(l.EmployeeId) && l.Status == LoanStatus.Active)
            .ToListAsync(ct);

        var epfEtfRule = await _epfEtfRuleSetService.GetActiveRuleForDateAsync(payRun.PayDate);
        var taxRuleSet = await _taxRuleSetService.GetActiveRuleForDateAsync(payRun.PayDate);

        var paySlips = new List<PaySlip>();

        foreach (var employee in employees)
        {
            var paySlip = await CalculatePaySlipForEmployeeAsync(
                payRun,
                employee,
                epfEtfRule,
                taxRuleSet,
                new PaySlipCalculationContext
                {
                    Employee = employee,
                    PaySlipId = Guid.NewGuid(),
                    Attendance = attendance.Where(a => a.EmployeeId == employee.Id).ToList(),
                    Overtime = overtime.Where(o => o.EmployeeId == employee.Id).ToList(),
                    ActiveLoans = loans.Where(l => l.EmployeeId == employee.Id).ToList()
                },
                ct);

            paySlip.PayRunId = payRun.Id;
            paySlips.Add(paySlip);
        }

        return paySlips;
    }

    private async Task<PaySlip> CalculatePaySlipForEmployeeAsync(
        PayRun payRun,
        Employee employee,
        EpfEtfRuleSetDto? epfEtfRule,
        TaxRuleSetDto? taxRuleSet,
        PaySlipCalculationContext ctx,
        CancellationToken ct)
    {
        ctx.BasicSalary = employee.BaseSalary;

        var paySlip = new PaySlip
        {
            Id = ctx.PaySlipId,
            EmployeeId = employee.Id,
            PayRunId = payRun.Id,
            CreatedBy = "system"
        };

        await ApplyBasicSalaryAsync(ctx, payRun);
        await ApplyNoPayDeductionsAsync(ctx, payRun);
        await ApplyOvertimeEarningsAsync(ctx, payRun);
        await ApplyFixedAllowancesAsync(ctx, payRun);
        await ApplyFixedDeductionsAsync(ctx, payRun);
        await ApplyLoansAsync(ctx, payRun);
        await ApplyStatutoryContributionsAsync(ctx, payRun, epfEtfRule, taxRuleSet);

        var totalEarnings = ctx.TotalEarnings;
        var totalDeductions = ctx.TotalDeductions;
        var netPay = RoundCurrency(totalEarnings - totalDeductions);

        paySlip.BasicSalary = RoundCurrency(ctx.BasicSalary);
        paySlip.TotalEarnings = RoundCurrency(totalEarnings);
        paySlip.TotalDeductions = RoundCurrency(totalDeductions);
        paySlip.NetPay = netPay;
        paySlip.EmployeeEpf = ctx.EmployeeEpf;
        paySlip.EmployerEpf = ctx.EmployerEpf;
        paySlip.EmployerEtf = ctx.EmployerEtf;
        paySlip.PayeTax = ctx.PayeTax;
        paySlip.Earnings = ctx.Earnings;
        paySlip.Deductions = ctx.Deductions;

        return paySlip;
    }

    private Task ApplyBasicSalaryAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        ctx.Earnings.Add(new EarningLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = ctx.PaySlipId,
            Code = "BASIC",
            Description = "Basic Salary",
            Amount = RoundCurrency(ctx.BasicSalary),
            IsEpfApplicable = true,
            IsEtfApplicable = true,
            IsTaxable = true
        });

        return Task.CompletedTask;
    }

    private Task ApplyNoPayDeductionsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var absentDays = ctx.Attendance.Count(a => a.HoursWorked <= 0);
        var dailyRate = RoundCurrency(ctx.BasicSalary / WorkingDaysPerMonth);
        var noPayAmount = RoundCurrency(dailyRate * absentDays);

        if (noPayAmount > 0)
        {
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "NOPAY",
                Description = "No Pay for Absences",
                Amount = noPayAmount,
                IsPreTax = true,
                IsPostTax = false
            });
        }

        return Task.CompletedTask;
    }

    private Task ApplyOvertimeEarningsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var baseHourlyRate = ctx.BasicSalary / (WorkingDaysPerMonth * WorkingHoursPerDay);

        foreach (var overtime in ctx.Overtime)
        {
            var multiplier = overtime.Type switch
            {
                OvertimeType.Weekend => WeekendOvertimeMultiplier,
                OvertimeType.PublicHoliday => PublicHolidayOvertimeMultiplier,
                _ => WeekdayOvertimeMultiplier
            };

            var otAmount = RoundCurrency((decimal)overtime.Hours * baseHourlyRate * multiplier);

            if (otAmount <= 0)
            {
                continue;
            }

            ctx.Earnings.Add(new EarningLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "OT",
                Description = $"Overtime ({overtime.Type})",
                Amount = otAmount,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true
            });
        }

        return Task.CompletedTask;
    }

    private Task ApplyFixedAllowancesAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        // TODO: Load employee-specific recurring allowances using AllowanceType configuration.
        return Task.CompletedTask;
    }

    private Task ApplyFixedDeductionsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        // TODO: Load employee-specific recurring deductions using DeductionType configuration.
        return Task.CompletedTask;
    }

    private Task ApplyLoansAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        foreach (var loan in ctx.ActiveLoans)
        {
            if (loan.Status != LoanStatus.Active || loan.Outstanding <= 0)
            {
                continue;
            }

            var installment = loan.Repayments.FirstOrDefault(r => !r.IsPaid)?.Amount ?? 0m;
            if (installment <= 0)
            {
                installment = loan.Principal > 0 ? loan.Principal / 12m : loan.Outstanding;
            }

            installment = Math.Min(loan.Outstanding, installment);
            installment = RoundCurrency(installment);

            if (installment <= 0)
            {
                continue;
            }

            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "LOAN",
                Description = "Loan Installment",
                Amount = installment,
                IsPreTax = true,
                IsPostTax = false
            });

            loan.Outstanding -= installment;
            if (loan.Outstanding <= 0)
            {
                loan.Status = LoanStatus.Closed;
            }
        }

        return Task.CompletedTask;
    }

    private Task ApplyStatutoryContributionsAsync(
        PaySlipCalculationContext ctx,
        PayRun payRun,
        EpfEtfRuleSetDto? epfEtfRule,
        TaxRuleSetDto? taxRuleSet)
    {
        ApplyEpfEtf(ctx, epfEtfRule);
        ApplyPaye(ctx, taxRuleSet);

        return Task.CompletedTask;
    }

    private void ApplyEpfEtf(PaySlipCalculationContext ctx, EpfEtfRuleSetDto? epfEtfRule)
    {
        if (epfEtfRule is null)
        {
            ctx.EmployeeEpf = 0;
            ctx.EmployerEpf = 0;
            ctx.EmployerEtf = 0;
            return;
        }

        var epfBase = ctx.Earnings.Where(e => e.IsEpfApplicable).Sum(e => e.Amount);
        var etfBase = ctx.Earnings.Where(e => e.IsEtfApplicable).Sum(e => e.Amount);

        if (epfEtfRule.MinimumWageForEpf.HasValue && epfBase < epfEtfRule.MinimumWageForEpf.Value)
        {
            epfBase = 0;
        }

        if (epfEtfRule.MaximumEarningForEpf.HasValue)
        {
            epfBase = Math.Min(epfBase, epfEtfRule.MaximumEarningForEpf.Value);
        }

        if (epfEtfRule.MaximumEarningForEtf.HasValue)
        {
            etfBase = Math.Min(etfBase, epfEtfRule.MaximumEarningForEtf.Value);
        }

        var employeeEpf = RoundCurrency(epfBase * epfEtfRule.EmployeeEpfRate / 100m);
        var employerEpf = RoundCurrency(epfBase * epfEtfRule.EmployerEpfRate / 100m);
        var employerEtf = RoundCurrency(etfBase * epfEtfRule.EmployerEtfRate / 100m);

        if (employeeEpf > 0)
        {
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "EPF_EE",
                Description = "Employee EPF",
                Amount = employeeEpf,
                IsPreTax = true,
                IsPostTax = false
            });
        }

        ctx.EmployeeEpf = employeeEpf;
        ctx.EmployerEpf = employerEpf;
        ctx.EmployerEtf = employerEtf;
    }

    private void ApplyPaye(PaySlipCalculationContext ctx, TaxRuleSetDto? taxRuleSet)
    {
        var taxableEarnings = ctx.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
        var preTaxDeductions = ctx.Deductions.Where(d => d.IsPreTax).Sum(d => d.Amount);
        var taxableIncome = taxableEarnings - preTaxDeductions;

        if (taxableIncome <= 0 || taxRuleSet is null || taxRuleSet.Slabs.Count == 0)
        {
            ctx.PayeTax = 0;
            return;
        }

        decimal totalTax = 0;
        var sortedSlabs = taxRuleSet.Slabs.OrderBy(s => s.Order).ToList();

        foreach (var slab in sortedSlabs)
        {
            if (taxableIncome <= slab.FromAmount)
            {
                continue;
            }

            var upperBound = slab.ToAmount ?? decimal.MaxValue;
            var chargeable = Math.Min(taxableIncome, upperBound) - slab.FromAmount;
            if (chargeable < 0)
            {
                chargeable = 0;
            }

            totalTax += chargeable * slab.RatePercent / 100m;
        }

        var paye = RoundCurrency(totalTax);

        if (paye > 0)
        {
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "PAYE",
                Description = "PAYE Tax",
                Amount = paye,
                IsPreTax = false,
                IsPostTax = true
            });
        }

        ctx.PayeTax = paye;
    }

    private static PayRunDto MapToDto(PayRun payRun)
    {
        return new PayRunDto
        {
            Id = payRun.Id,
            Reference = payRun.Reference,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate.ToDateTime(TimeOnly.MinValue),
            Status = payRun.Status,
            IsLocked = payRun.IsLocked,
            PaySlips = payRun.PaySlips.Select(MapToDto).ToList()
        };
    }

    private static PaySlipDto MapToDto(PaySlip paySlip)
    {
        return new PaySlipDto
        {
            Id = paySlip.Id,
            EmployeeId = paySlip.EmployeeId,
            BasicSalary = paySlip.BasicSalary,
            TotalEarnings = paySlip.TotalEarnings,
            TotalDeductions = paySlip.TotalDeductions,
            NetPay = paySlip.NetPay,
            EmployeeEpf = paySlip.EmployeeEpf,
            EmployerEpf = paySlip.EmployerEpf,
            EmployerEtf = paySlip.EmployerEtf,
            PayeTax = paySlip.PayeTax,
            Earnings = paySlip.Earnings.Select(e => new EarningDto(e.Code, e.Description, e.Amount, e.IsEpfApplicable, e.IsEtfApplicable, e.IsTaxable)).ToList(),
            Deductions = paySlip.Deductions.Select(d => new DeductionDto(d.Code, d.Description, d.Amount, d.IsPreTax, d.IsPostTax)).ToList()
        };
    }

    private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed class PaySlipCalculationContext
    {
        public Guid PaySlipId { get; init; }
        public Employee Employee { get; init; } = null!;
        public decimal BasicSalary { get; set; }
        public List<AttendanceRecord> Attendance { get; init; } = new();
        public List<OvertimeRecord> Overtime { get; init; } = new();
        public List<Loan> ActiveLoans { get; init; } = new();
        public List<EarningLine> Earnings { get; } = new();
        public List<DeductionLine> Deductions { get; } = new();
        public decimal EmployeeEpf { get; set; }
        public decimal EmployerEpf { get; set; }
        public decimal EmployerEtf { get; set; }
        public decimal PayeTax { get; set; }
        public decimal TotalEarnings => Earnings.Sum(x => x.Amount);
        public decimal TotalDeductions => Deductions.Sum(x => x.Amount);
    }

    // TODO: Add integration tests to cover basic, overtime, and statutory calculation scenarios.
}
