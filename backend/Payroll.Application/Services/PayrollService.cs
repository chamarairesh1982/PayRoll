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
    private readonly IAuditLogger _auditLogger;

    public PayrollService(
        IPayrollDbContext dbContext,
        IEpfEtfRuleSetService epfEtfRuleSetService,
        ITaxRuleSetService taxRuleSetService,
        IAuditLogger auditLogger)
    {
        _dbContext = dbContext;
        _epfEtfRuleSetService = epfEtfRuleSetService;
        _taxRuleSetService = taxRuleSetService;
        _auditLogger = auditLogger;
    }

    public async Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        var payRun = new PayRun
        {
            Reference = $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Code = request.Name,
            Name = request.Name,
            PeriodType = request.PeriodType,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            PayDate = request.PayDate == default ? request.PeriodEnd : request.PayDate,
            Status = PayRunStatus.Draft,
            IsLocked = false
        };

        var employeeIds = request.EmployeeIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        if (!employeeIds.Any())
        {
            employeeIds = await _dbContext.Employees
                .AsNoTracking()
                .Where(e => !request.IncludeActiveEmployeesOnly || e.IsActive)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        payRun.Status = PayRunStatus.Calculated;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunCalculated",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            null,
            cancellationToken);

        await _dbContext.PayRuns.AddAsync(payRun, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(payRun);
    }

    public async Task RecalculatePayRunAsync(Guid id, RecalculatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status == PayRunStatus.Locked || payRun.IsLocked)
        {
            throw new InvalidOperationException("Cannot recalculate a locked pay run.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).ToList();

        _dbContext.PaySlips.RemoveRange(payRun.PaySlips);
        payRun.PaySlips.Clear();

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        payRun.Status = PayRunStatus.Calculated;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunRecalculated",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            null,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return;
    }

    public async Task ChangeStatusAsync(Guid id, ChangePayRunStatusRequest request, CancellationToken cancellationToken = default)
    {
        switch (request.Status)
        {
            case PayRunStatus.Approved:
                await ApprovePayRunAsync(id, new PayRunActionRequest { ActionedBy = "system" }, cancellationToken);
                break;
            case PayRunStatus.Locked:
                await LockPayRunAsync(id, new PayRunActionRequest { ActionedBy = "system" }, cancellationToken);
                break;
            case PayRunStatus.Calculated:
                await ResetToCalculatedAsync(id, cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unsupported pay run status transition.");
        }
    }

    public async Task ApprovePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status != PayRunStatus.Calculated)
        {
            throw new InvalidOperationException("Only calculated pay runs can be approved.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        AddApprovalLog(payRun, PayRunStatus.Calculated, PayRunStatus.Approved, request);
        payRun.Status = PayRunStatus.Approved;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunApproved",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            request.ActionedBy,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status != PayRunStatus.Approved)
        {
            throw new InvalidOperationException("Only approved pay runs can be locked.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        AddApprovalLog(payRun, PayRunStatus.Approved, PayRunStatus.Locked, request);
        payRun.Status = PayRunStatus.Locked;
        payRun.IsLocked = true;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunLocked",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            request.ActionedBy,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status != PayRunStatus.Locked)
        {
            throw new InvalidOperationException("Only locked pay runs can be unlocked.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        AddApprovalLog(payRun, PayRunStatus.Locked, PayRunStatus.Approved, request);
        payRun.Status = PayRunStatus.Approved;
        payRun.IsLocked = false;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunUnlocked",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            request.ActionedBy,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ResetToCalculatedAsync(Guid id, CancellationToken cancellationToken)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status == PayRunStatus.Locked || payRun.IsLocked)
        {
            throw new InvalidOperationException("Cannot reset status for a locked pay run.");
        }

        if (payRun.Status != PayRunStatus.Calculated)
        {
            AddApprovalLog(payRun, payRun.Status, PayRunStatus.Calculated, new PayRunActionRequest { ActionedBy = "system" });
        }

        payRun.Status = PayRunStatus.Calculated;
        payRun.IsLocked = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddApprovalLog(PayRun payRun, PayRunStatus fromStatus, PayRunStatus toStatus, PayRunActionRequest request)
    {
        var actionedBy = string.IsNullOrWhiteSpace(request.ActionedBy) ? "Unknown" : request.ActionedBy;

        payRun.Approvals.Add(new PayRunApproval
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActionedBy = actionedBy,
            Comments = request.Comments,
            ActionedAt = DateTime.UtcNow
        });
    }

    private static object CreatePayRunSnapshot(PayRun payRun)
    {
        return new
        {
            payRun.Id,
            payRun.Status,
            payRun.IsLocked,
            payRun.PeriodStart,
            payRun.PeriodEnd,
            payRun.PayDate,
            PaySlipCount = payRun.PaySlips.Count,
            PaySlips = payRun.PaySlips.Select(ps => new
            {
                ps.Id,
                ps.EmployeeId,
                ps.BasicSalary,
                ps.TotalEarnings,
                ps.TotalDeductions,
                ps.NetPay
            }).ToList()
        };
    }

    public async Task<PayRunDetailDto?> GetPayRunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .Include(pr => pr.Approvals)
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        return payRun is null ? null : MapToDetailDto(payRun);
    }

    public async Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(int page, int pageSize, PayRunStatus? status = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var query = _dbContext.PayRuns.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status);
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PayRunSummaryDto>
        {
            Items = items.Select(MapToSummaryDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaySlipDto?> GetPaySlipAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var paySlip = await _dbContext.PaySlips
            .Include(ps => ps.Earnings)
            .Include(ps => ps.Deductions)
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.Id == paySlipId && ps.PayRunId == payRunId, cancellationToken);

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
                        && a.Period.Start <= periodEnd
                        && a.Period.End >= periodStart)
            .ToListAsync(ct);

        var overtime = await _dbContext.OvertimeRecords
            .Where(o => employeeIds.Contains(o.EmployeeId)
                        && o.Date >= periodStart
                        && o.Date <= periodEnd
                        && o.Status == OvertimeStatus.Approved
                        && !o.IsLockedForPayroll)
            .ToListAsync(ct);

        var loans = await _dbContext.Loans
            .Include(l => l.Repayments)
            .Where(l => employeeIds.Contains(l.EmployeeId) && l.Status == LoanStatus.Active)
            .ToListAsync(ct);

        var allowanceTypes = await _dbContext.AllowanceTypes
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Code, ct);

        var deductionTypes = await _dbContext.DeductionTypes
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Code, ct);

        var payItems = await _dbContext.EmployeePayItems
            .AsNoTracking()
            .Where(pi => employeeIds.Contains(pi.EmployeeId)
                        && pi.IsActive
                        && pi.EffectiveFrom <= periodEnd
                        && (pi.EffectiveTo == null || pi.EffectiveTo >= periodStart))
            .ToListAsync(ct);

        var recurringPayItems = await _dbContext.EmployeeRecurringPayItems
            .AsNoTracking()
            .Include(pi => pi.AllowanceType)
            .Include(pi => pi.DeductionType)
            .Where(pi => employeeIds.Contains(pi.EmployeeId)
                        && pi.IsActive
                        && pi.EffectiveFrom <= periodEnd
                        && (pi.EffectiveTo == null || pi.EffectiveTo >= periodStart))
            .ToListAsync(ct);

        var payDateOnly = DateOnly.FromDateTime(payRun.PayDate);
        var epfEtfRule = await _epfEtfRuleSetService.GetActiveRuleForDateAsync(payDateOnly);
        var taxRuleSet = await _taxRuleSetService.GetActiveRuleForDateAsync(payDateOnly);

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
                    ActiveLoans = loans.Where(l => l.EmployeeId == employee.Id).ToList(),
                    PayItems = payItems.Where(pi => pi.EmployeeId == employee.Id).ToList(),
                    RecurringPayItems = recurringPayItems.Where(pi => pi.EmployeeId == employee.Id).ToList(),
                    AllowanceTypes = allowanceTypes,
                    DeductionTypes = deductionTypes
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
        var allowanceLines = ctx.PayItems
            .Where(pi => pi.PayItemType == PayItemType.Allowance)
            .Select(pi => (AllowanceType: ctx.AllowanceTypes.TryGetValue(pi.PayItemCode, out var allowanceType)
                ? allowanceType
                : null,
                Amount: ResolvePayItemAmount(pi, ctx.BasicSalary)))
            .Concat(ctx.RecurringPayItems
                .Where(pi => pi.PayItemKind == PayItemKind.Allowance)
                .Select(pi => (AllowanceType: pi.AllowanceType, Amount: ResolveRecurringPayItemAmount(pi, ctx.BasicSalary))));

        foreach (var allowance in allowanceLines)
        {
            if (allowance.AllowanceType is null)
            {
                continue;
            }

            if (allowance.Amount <= 0)
            {
                continue;
            }

            ctx.Earnings.Add(new EarningLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = allowance.AllowanceType.Code,
                Description = allowance.AllowanceType.Name,
                Amount = allowance.Amount,
                IsEpfApplicable = allowance.AllowanceType.IsEpfApplicable,
                IsEtfApplicable = allowance.AllowanceType.IsEtfApplicable,
                IsTaxable = allowance.AllowanceType.IsTaxable
            });
        }

        return Task.CompletedTask;
    }

    private Task ApplyFixedDeductionsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var deductionLines = ctx.PayItems
            .Where(pi => pi.PayItemType == PayItemType.Deduction)
            .Select(pi => (DeductionType: ctx.DeductionTypes.TryGetValue(pi.PayItemCode, out var deductionType)
                ? deductionType
                : null,
                Amount: ResolvePayItemAmount(pi, ctx.BasicSalary)))
            .Concat(ctx.RecurringPayItems
                .Where(pi => pi.PayItemKind == PayItemKind.Deduction)
                .Select(pi => (DeductionType: pi.DeductionType, Amount: ResolveRecurringPayItemAmount(pi, ctx.BasicSalary))));

        foreach (var deduction in deductionLines)
        {
            if (deduction.DeductionType is null)
            {
                continue;
            }

            if (deduction.Amount <= 0)
            {
                continue;
            }

            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = deduction.DeductionType.Code,
                Description = deduction.DeductionType.Name,
                Amount = deduction.Amount,
                IsPreTax = deduction.DeductionType.IsPreTax,
                IsPostTax = deduction.DeductionType.IsPostTax
            });
        }

        return Task.CompletedTask;
    }

    private Task ApplyLoansAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        foreach (var loan in ctx.ActiveLoans)
        {
            if (loan.Status != LoanStatus.Active || loan.OutstandingPrincipal <= 0)
            {
                continue;
            }

            var installment = loan.Repayments.FirstOrDefault(r => !r.IsPaid)?.Amount ?? loan.InstallmentAmount;

            installment = Math.Min(loan.OutstandingPrincipal, installment);
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

            loan.OutstandingPrincipal -= installment;
            if (loan.OutstandingPrincipal <= 0)
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

    private static PayRunSummaryDto MapToSummaryDto(PayRun payRun)
    {
        var employeeCount = payRun.PaySlips.Count;
        var totalNet = payRun.PaySlips.Sum(ps => ps.NetPay);

        return new PayRunSummaryDto
        {
            Id = payRun.Id,
            Code = payRun.Code,
            Name = payRun.Name,
            PeriodType = payRun.PeriodType,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate,
            Status = payRun.Status,
            IsLocked = payRun.IsLocked,
            EmployeeCount = employeeCount,
            TotalNetPay = totalNet
        };
    }

    private static PayRunDetailDto MapToDetailDto(PayRun payRun)
    {
        var summary = MapToSummaryDto(payRun);
        return new PayRunDetailDto
        {
            Id = summary.Id,
            Code = summary.Code,
            Name = summary.Name,
            PeriodType = summary.PeriodType,
            PeriodStart = summary.PeriodStart,
            PeriodEnd = summary.PeriodEnd,
            PayDate = summary.PayDate,
            Status = summary.Status,
            IsLocked = summary.IsLocked,
            EmployeeCount = summary.EmployeeCount,
            TotalNetPay = summary.TotalNetPay,
            PaySlips = payRun.PaySlips.Select(MapToDto).ToList(),
            Approvals = payRun.Approvals
                .OrderByDescending(a => a.ActionedAt)
                .Select(MapToDto)
                .ToList()
        };
    }

    private static PayRunApprovalDto MapToDto(PayRunApproval approval)
    {
        return new PayRunApprovalDto
        {
            Id = approval.Id,
            FromStatus = approval.FromStatus,
            ToStatus = approval.ToStatus,
            ActionedBy = approval.ActionedBy,
            Comments = approval.Comments,
            ActionedAt = approval.ActionedAt
        };
    }

    private static PaySlipDto MapToDto(PaySlip paySlip)
    {
        return new PaySlipDto
        {
            Id = paySlip.Id,
            PayRunId = paySlip.PayRunId,
            EmployeeId = paySlip.EmployeeId,
            EmployeeCode = paySlip.Employee?.Code,
            EmployeeName = paySlip.Employee?.FullName,
            Currency = "LKR",
            BasicSalary = paySlip.BasicSalary,
            TotalEarnings = paySlip.TotalEarnings,
            TotalDeductions = paySlip.TotalDeductions,
            NetPay = paySlip.NetPay,
            EmployeeEpf = paySlip.EmployeeEpf,
            EmployerEpf = paySlip.EmployerEpf,
            EmployerEtf = paySlip.EmployerEtf,
            PayeTax = paySlip.PayeTax,
            Earnings = paySlip.Earnings.Select(e => new EarningDto(e.Id, e.Code, e.Description, e.Amount, e.IsEpfApplicable, e.IsEtfApplicable, e.IsTaxable)).ToList(),
            Deductions = paySlip.Deductions.Select(d => new DeductionDto(d.Id, d.Code, d.Description, d.Amount, d.IsPreTax, d.IsPostTax)).ToList()
        };
    }

    private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private decimal ResolvePayItemAmount(EmployeePayItem payItem, decimal basicSalary)
    {
        if (payItem.Amount.HasValue)
        {
            return RoundCurrency(payItem.Amount.Value);
        }

        if (payItem.Percentage.HasValue)
        {
            return RoundCurrency(basicSalary * payItem.Percentage.Value / 100m);
        }

        return 0;
    }

    private decimal ResolveRecurringPayItemAmount(EmployeeRecurringPayItem payItem, decimal basicSalary)
    {
        if (payItem.Amount.HasValue)
        {
            return RoundCurrency(payItem.Amount.Value);
        }

        if (payItem.Percentage.HasValue)
        {
            return RoundCurrency(basicSalary * payItem.Percentage.Value / 100m);
        }

        return 0;
    }

    private sealed class PaySlipCalculationContext
    {
        public Guid PaySlipId { get; init; }
        public Employee Employee { get; init; } = null!;
        public decimal BasicSalary { get; set; }
        public List<AttendanceRecord> Attendance { get; init; } = new();
        public List<OvertimeRecord> Overtime { get; init; } = new();
        public List<Loan> ActiveLoans { get; init; } = new();
        public List<EmployeePayItem> PayItems { get; init; } = new();
        public List<EmployeeRecurringPayItem> RecurringPayItems { get; init; } = new();
        public IReadOnlyDictionary<string, AllowanceType> AllowanceTypes { get; init; } = new Dictionary<string, AllowanceType>();
        public IReadOnlyDictionary<string, DeductionType> DeductionTypes { get; init; } = new Dictionary<string, DeductionType>();
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
