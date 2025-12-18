using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.BankExports;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Application.Exceptions;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class PayrollService : IPayrollService
{
    private const int DefaultWorkingDaysPerMonth = 26;
    private const int DefaultWorkingHoursPerDay = 8;
    private const decimal DefaultWeekdayOvertimeMultiplier = 1.5m;
    private const decimal DefaultWeekendOvertimeMultiplier = 2.0m;
    private const decimal DefaultPublicHolidayOvertimeMultiplier = 2.0m;

    private readonly IPayrollDbContext _dbContext;
    private readonly IEpfEtfRuleSetService _epfEtfRuleSetService;
    private readonly ITaxRuleSetService _taxRuleSetService;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly BankExportTemplateResolver _bankExportResolver = new();

    public PayrollService(
        IPayrollDbContext dbContext,
        IEpfEtfRuleSetService epfEtfRuleSetService,
        ITaxRuleSetService taxRuleSetService,
        IAuditLogger auditLogger,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _epfEtfRuleSetService = epfEtfRuleSetService;
        _taxRuleSetService = taxRuleSetService;
        _auditLogger = auditLogger;
        _currentUserService = currentUserService;
    }

    public async Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.IsConsolidated && request.CompanyId == null && request.BranchId == null && request.CostCenterId == null)
        {
            throw new InvalidOperationException("Non-consolidated pay runs must target a company, branch, or cost center.");
        }

        var validatedScope = await ValidateOrganizationScopeAsync(
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            cancellationToken);

        var payRun = new PayRun
        {
            Reference = $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Code = request.Name,
            Name = request.Name,
            PeriodType = request.PeriodType,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            PayDate = request.PayDate == default ? request.PeriodEnd : request.PayDate,
            CompanyId = validatedScope.CompanyId,
            BranchId = validatedScope.BranchId,
            CostCenterId = validatedScope.CostCenterId,
            IsConsolidated = request.IsConsolidated,
            Status = PayRunStatus.Draft,
            IsLocked = false
        };

        var employeeIds = request.EmployeeIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        if (!employeeIds.Any())
        {
            var employeesQuery = ApplyScopeFilter(_dbContext.Employees.AsNoTracking(), payRun.CompanyId, payRun.BranchId, payRun.CostCenterId);

            if (request.IncludeActiveEmployeesOnly)
            {
                employeesQuery = employeesQuery.Where(e => e.IsActive);
            }

            employeeIds = await employeesQuery.Select(e => e.Id).ToListAsync(cancellationToken);
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunCreated",
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

        if (payRun.Status == PayRunStatus.Approved)
        {
            throw new InvalidOperationException("Cannot recalculate an approved pay run. Unlock and move it back to draft before recalculating.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).ToList();

        _dbContext.PaySlips.RemoveRange(payRun.PaySlips);
        payRun.PaySlips.Clear();

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        var recalculatedStatus = payRun.Status == PayRunStatus.Prepared ? PayRunStatus.Prepared : PayRunStatus.Draft;
        payRun.Status = recalculatedStatus;
        payRun.ExportStatus = BankExportStatus.Pending;
        payRun.ExportedBank = null;
        payRun.ExportedAt = null;
        payRun.ExportDownloadedAt = null;

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
            case PayRunStatus.Prepared:
                await PreparePayRunAsync(id, new PayRunActionRequest(), cancellationToken);
                break;
            case PayRunStatus.Approved:
                await ApprovePayRunAsync(id, new PayRunActionRequest(), cancellationToken);
                break;
            case PayRunStatus.Locked:
                await LockPayRunAsync(id, new PayRunActionRequest(), cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unsupported pay run status transition.");
        }
    }

    public async Task PreparePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Maker", "prepare pay runs");

        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status == PayRunStatus.Locked || payRun.IsLocked)
        {
            throw new InvalidOperationException("Cannot prepare a locked pay run.");
        }

        if (payRun.Status != PayRunStatus.Draft)
        {
            throw new InvalidOperationException("Only draft pay runs can be prepared.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var actor = GetActor();
        AddStatusHistory(payRun, PayRunStatus.Draft, PayRunStatus.Prepared, actor, request.Comment);
        payRun.PreparedAt = DateTime.UtcNow;
        payRun.PreparedByUserId = actor.UserId;
        payRun.PreparedByUserName = actor.UserName;
        payRun.Status = PayRunStatus.Prepared;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunPrepared",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApprovePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "approve pay runs");

        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status != PayRunStatus.Prepared)
        {
            throw new InvalidOperationException("Only prepared pay runs can be approved.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new InvalidOperationException("Approval comment is required.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var actor = GetActor();
        AddStatusHistory(payRun, PayRunStatus.Prepared, PayRunStatus.Approved, actor, request.Comment);
        payRun.ApprovedAt = DateTime.UtcNow;
        payRun.ApprovedByUserId = actor.UserId;
        payRun.ApprovedByUserName = actor.UserName;
        payRun.Status = PayRunStatus.Approved;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunApproved",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "lock pay runs");

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

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new InvalidOperationException("Lock comment is required.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var actor = GetActor();
        AddStatusHistory(payRun, PayRunStatus.Approved, PayRunStatus.Locked, actor, request.Comment);
        payRun.LockedAt = DateTime.UtcNow;
        payRun.LockedByUserId = actor.UserId;
        payRun.LockedByUserName = actor.UserName;
        payRun.Status = PayRunStatus.Locked;
        payRun.IsLocked = true;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunLocked",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Unlocking pay runs is not permitted in the current workflow.");
    }

    private void AddStatusHistory(PayRun payRun, PayRunStatus fromStatus, PayRunStatus toStatus, (string UserId, string UserName) actor, string? comment)
    {
        payRun.Approvals.Add(new PayRunApproval
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActorUserId = string.IsNullOrWhiteSpace(actor.UserId) ? null : actor.UserId,
            ActorUserName = actor.UserName,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            ActionedAt = DateTime.UtcNow
        });
    }

    private (string UserId, string UserName) GetActor()
    {
        var userName = string.IsNullOrWhiteSpace(_currentUserService.UserName) ? "Unknown" : _currentUserService.UserName!;
        var userId = _currentUserService.UserId ?? string.Empty;
        return (userId, userName);
    }

    private void EnsureRole(string role, string action)
    {
        var hasRole = _currentUserService.Roles?.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)) == true;
        if (!hasRole)
        {
            throw new ForbiddenAccessException($"Only users with the {role} role can {action}.");
        }
    }

    private static object CreatePayRunSnapshot(PayRun payRun)
    {
        return new
        {
            payRun.Id,
            payRun.Status,
            payRun.IsLocked,
            payRun.IsConsolidated,
            payRun.CompanyId,
            payRun.BranchId,
            payRun.CostCenterId,
            payRun.PreparedAt,
            payRun.PreparedByUserName,
            payRun.ApprovedAt,
            payRun.ApprovedByUserName,
            payRun.LockedAt,
            payRun.LockedByUserName,
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
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .Include(pr => pr.Approvals)
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        return payRun is null ? null : MapToDetailDto(payRun);
    }

    public async Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(PayRunQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);

        var payRuns = _dbContext.PayRuns.AsNoTracking();
        if (query.Status.HasValue)
        {
            payRuns = payRuns.Where(p => p.Status == query.Status);
        }

        if (query.CompanyId.HasValue)
        {
            payRuns = payRuns.Where(p => p.CompanyId == query.CompanyId);
        }

        if (query.BranchId.HasValue)
        {
            payRuns = payRuns.Where(p => p.BranchId == query.BranchId);
        }

        if (query.CostCenterId.HasValue)
        {
            payRuns = payRuns.Where(p => p.CostCenterId == query.CostCenterId);
        }

        if (query.IsConsolidated.HasValue)
        {
            payRuns = payRuns.Where(p => p.IsConsolidated == query.IsConsolidated.Value);
        }

        var totalCount = await payRuns.CountAsync(cancellationToken);
        var items = await payRuns
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
            .Include(ps => ps.Employee)
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.Id == paySlipId && ps.PayRunId == payRunId, cancellationToken);

        return paySlip is null ? null : MapToDto(paySlip);
    }

    public async Task<BankExportResultDto> GenerateBankExportAsync(Guid payRunId, BankExportRequest request, CancellationToken cancellationToken = default)
    {
        var template = _bankExportResolver.Resolve(request.Bank);
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        var failures = new List<BankExportFailureDto>();
        var rows = new List<BankExportRow>();

        foreach (var paySlip in payRun.PaySlips)
        {
            var employee = paySlip.Employee;
            if (employee is null)
            {
                failures.Add(new BankExportFailureDto
                {
                    EmployeeId = paySlip.EmployeeId,
                    Reason = "Employee details unavailable"
                });
                continue;
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(employee.BankAccountNumber))
            {
                missingFields.Add("account number");
            }

            if (string.IsNullOrWhiteSpace(employee.BranchCode))
            {
                missingFields.Add("branch code");
            }

            if (string.IsNullOrWhiteSpace(employee.BankCode))
            {
                missingFields.Add("bank code");
            }

            if (string.IsNullOrWhiteSpace(employee.BankName))
            {
                missingFields.Add("bank name");
            }

            if (missingFields.Any())
            {
                failures.Add(new BankExportFailureDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    Reason = $"Missing bank details: {string.Join(", ", missingFields)}"
                });
                continue;
            }

            rows.Add(new BankExportRow(
                employee.EmployeeCode,
                employee.FullName,
                employee.BankAccountNumber!,
                employee.BranchCode!,
                paySlip.NetPay));
        }

        if (failures.Any())
        {
            return new BankExportResultDto
            {
                Status = payRun.ExportStatus,
                Bank = request.Bank,
                Failures = failures
            };
        }

        var reference = string.IsNullOrWhiteSpace(payRun.Reference) ? payRun.Code : payRun.Reference;
        var content = template.Render(rows, reference);
        var fileName = $"{payRun.Code}-{template.Bank}-{DateTime.UtcNow:yyyyMMddHHmmss}.{template.FileExtension}";

        payRun.ExportStatus = BankExportStatus.Generated;
        payRun.ExportedBank = template.Bank;
        payRun.ExportedAt = DateTime.UtcNow;
        payRun.ExportDownloadedAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BankExportResultDto
        {
            Status = payRun.ExportStatus,
            Bank = template.Bank,
            FileName = fileName,
            ContentType = template.ContentType,
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
            Failures = failures
        };
    }

    public async Task MarkBankExportDownloadedAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns.FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        payRun.ExportStatus = BankExportStatus.Downloaded;
        payRun.ExportDownloadedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<PaySlip>> GeneratePaySlipsForPayRunAsync(PayRun payRun, List<Guid> employeeIds, CancellationToken ct)
    {
        var employees = await ApplyScopeFilter(_dbContext.Employees, payRun.CompanyId, payRun.BranchId, payRun.CostCenterId)
            .Where(e => employeeIds.Contains(e.Id) && e.IsActive)
            .ToListAsync(ct);

        var periodStart = DateOnly.FromDateTime(payRun.PeriodStart);
        var periodEnd = DateOnly.FromDateTime(payRun.PeriodEnd);

        var attendance = await _dbContext.AttendanceRecords
            .Where(a => employeeIds.Contains(a.EmployeeId)
                        && a.Period.Start <= periodEnd
                        && a.Period.End >= periodStart)
            .ToListAsync(ct);

        var payrollSettings = await GetPayrollSettingsAsync(ct);

        var overtime = await _dbContext.OvertimeRecords
            .Where(o => employeeIds.Contains(o.EmployeeId)
                        && o.Date >= periodStart
                        && o.Date <= periodEnd
                        && o.Status == OvertimeStatus.Approved
                        && (!o.IsLockedForPayroll || o.PayRunId == payRun.Id)
                        && (o.PayRunId == null || o.PayRunId == payRun.Id))
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

        var recurringRules = await _dbContext.RecurringRules
            .AsNoTracking()
            .Where(r => r.IsActive
                        && r.Frequency == payRun.PeriodType
                        && r.StartDate <= DateOnly.FromDateTime(periodEnd)
                        && (r.EndDate == null || r.EndDate >= DateOnly.FromDateTime(periodStart)))
            .ToListAsync(ct);

        var leaveRequests = await _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(lr => employeeIds.Contains(lr.EmployeeId)
                         && lr.IsActive
                         && lr.Status == LeaveStatus.Approved
                         && lr.StartDate <= periodEnd
                         && lr.EndDate >= periodStart)
            .ToListAsync(ct);

        var payDateOnly = DateOnly.FromDateTime(payRun.PayDate);
        var epfEtfRule = await _epfEtfRuleSetService.GetActiveRuleForDateAsync(payDateOnly);
        var taxRuleSet = await _taxRuleSetService.GetActiveRuleForDateAsync(payDateOnly);

        var paySlips = new List<PaySlip>();

        var recurringKeySet = new HashSet<string>();

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
                    RecurringRules = recurringRules.Where(r => r.EmployeeId == employee.Id).ToList(),
                    LeaveRequests = leaveRequests.Where(lr => lr.EmployeeId == employee.Id).ToList(),
                    AllowanceTypes = allowanceTypes,
                    DeductionTypes = deductionTypes,
                    WorkingDaysPerMonth = payrollSettings.WorkingDaysPerMonth,
                    WorkingHoursPerDay = payrollSettings.WorkingHoursPerDay,
                    WeekdayOvertimeMultiplier = payrollSettings.WeekdayOvertimeMultiplier,
                    WeekendOvertimeMultiplier = payrollSettings.WeekendOvertimeMultiplier,
                    HolidayOvertimeMultiplier = payrollSettings.HolidayOvertimeMultiplier
                },
                recurringKeySet,
                ct);

            paySlip.PayRunId = payRun.Id;
            paySlips.Add(paySlip);
        }

        return paySlips;
    }

    private async Task<PayrollSettingsSnapshot> GetPayrollSettingsAsync(CancellationToken ct)
    {
        var settings = await _dbContext.PayrollSettings.AsNoTracking().FirstOrDefaultAsync(ct);

        return new PayrollSettingsSnapshot
        {
            WorkingDaysPerMonth = settings?.WorkingDaysPerMonth ?? DefaultWorkingDaysPerMonth,
            WorkingHoursPerDay = settings?.WorkingHoursPerDay ?? DefaultWorkingHoursPerDay,
            WeekdayOvertimeMultiplier = settings?.WeekdayOvertimeMultiplier ?? DefaultWeekdayOvertimeMultiplier,
            WeekendOvertimeMultiplier = settings?.WeekendOvertimeMultiplier ?? DefaultWeekendOvertimeMultiplier,
            HolidayOvertimeMultiplier = settings?.HolidayOvertimeMultiplier ?? DefaultPublicHolidayOvertimeMultiplier
        };
    }

    private async Task<PaySlip> CalculatePaySlipForEmployeeAsync(
        PayRun payRun,
        Employee employee,
        EpfEtfRuleSetDto? epfEtfRule,
        TaxRuleSetDto? taxRuleSet,
        PaySlipCalculationContext ctx,
        HashSet<string> recurringKeys,
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
        await ApplyRecurringRulesAsync(ctx, payRun, recurringKeys);
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
        var periodStart = DateOnly.FromDateTime(payRun.PeriodStart);
        var periodEnd = DateOnly.FromDateTime(payRun.PeriodEnd);

        var absentDayUnits = BuildAbsentDayUnits(ctx.Attendance, periodStart, periodEnd);
        var encashableAbsences = new Dictionary<DateOnly, decimal>(absentDayUnits);

        var dailyRate = RoundCurrency(ctx.BasicSalary / ctx.WorkingDaysPerMonth);

        foreach (var leave in ctx.LeaveRequests)
        {
            var leaveDayUnits = GetOverlappingLeaveDayUnits(leave, periodStart, periodEnd);
            if (leaveDayUnits.Count == 0)
            {
                continue;
            }

            var leaveDays = leaveDayUnits.Values.Sum();
            var (overlapStart, overlapEnd) = GetOverlapRange(leave, periodStart, periodEnd);

            if (leave.LeaveType == LeaveTypeCode.NoPay)
            {
                var noPayAmount = RoundCurrency(dailyRate * leaveDays);
                if (noPayAmount > 0)
                {
                    ctx.Deductions.Add(new DeductionLine
                    {
                        Id = Guid.NewGuid(),
                        PaySlipId = ctx.PaySlipId,
                        Code = "LEAVE_NOPAY",
                        Description = $"No Pay Leave ({leave.LeaveType} {overlapStart:yyyy-MM-dd} to {overlapEnd:yyyy-MM-dd}, Ref: {leave.Id})",
                        Amount = noPayAmount,
                        IsPreTax = true,
                        IsPostTax = false
                    });
                }

                foreach (var unit in leaveDayUnits)
                {
                    ReduceDayUnit(absentDayUnits, unit.Key, unit.Value);
                    ReduceDayUnit(encashableAbsences, unit.Key, unit.Value);
                }
            }
            else
            {
                var encashUnitsForLeave = 0m;

                foreach (var unit in leaveDayUnits)
                {
                    if (!encashableAbsences.TryGetValue(unit.Key, out var available))
                    {
                        continue;
                    }

                    var encashUnits = Math.Min(unit.Value, available);
                    encashUnitsForLeave += encashUnits;

                    ReduceDayUnit(encashableAbsences, unit.Key, encashUnits);
                }

                if (encashUnitsForLeave > 0)
                {
                    ctx.Earnings.Add(new EarningLine
                    {
                        Id = Guid.NewGuid(),
                        PaySlipId = ctx.PaySlipId,
                        Code = "LEAVE_ENCASH",
                        Description = $"Leave Encashment ({leave.LeaveType} {overlapStart:yyyy-MM-dd} to {overlapEnd:yyyy-MM-dd}, Ref: {leave.Id})",
                        Amount = RoundCurrency(dailyRate * encashUnitsForLeave),
                        IsEpfApplicable = false,
                        IsEtfApplicable = false,
                        IsTaxable = false
                    });
                }
            }
        }

        var remainingAbsentUnits = absentDayUnits.Values.Sum();
        var noPayAmountForAttendance = RoundCurrency(dailyRate * remainingAbsentUnits);

        if (noPayAmountForAttendance > 0)
        {
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "NOPAY",
                Description = $"No Pay for Absences ({remainingAbsentUnits:0.##} days)",
                Amount = noPayAmountForAttendance,
                IsPreTax = true,
                IsPostTax = false
            });
        }

        return Task.CompletedTask;
    }

    private static Dictionary<DateOnly, decimal> BuildAbsentDayUnits(IEnumerable<AttendanceRecord> attendance, DateOnly periodStart, DateOnly periodEnd)
    {
        var absences = new Dictionary<DateOnly, decimal>();

        foreach (var record in attendance.Where(a => a.HoursWorked <= 0))
        {
            var overlapStart = record.Period.Start > periodStart ? record.Period.Start : periodStart;
            var overlapEnd = record.Period.End < periodEnd ? record.Period.End : periodEnd;

            if (overlapEnd < overlapStart)
            {
                continue;
            }

            foreach (var day in EnumerateDays(overlapStart, overlapEnd))
            {
                absences[day] = absences.TryGetValue(day, out var existing) ? existing + 1 : 1;
            }
        }

        return absences;
    }

    private static Dictionary<DateOnly, decimal> GetOverlappingLeaveDayUnits(LeaveRequest leave, DateOnly periodStart, DateOnly periodEnd)
    {
        var overlapStart = leave.StartDate > periodStart ? leave.StartDate : periodStart;
        var overlapEnd = leave.EndDate < periodEnd ? leave.EndDate : periodEnd;

        if (overlapEnd < overlapStart)
        {
            return new Dictionary<DateOnly, decimal>();
        }

        var requestedUnits = leave.TotalDays > 0 ? (decimal)leave.TotalDays : overlapEnd.DayNumber - overlapStart.DayNumber + 1;

        if (leave.IsHalfDay == true)
        {
            requestedUnits = Math.Min(requestedUnits, 0.5m);
        }

        var overlapDays = overlapEnd.DayNumber - overlapStart.DayNumber + 1;
        requestedUnits = Math.Min(requestedUnits, overlapDays);

        var unitsByDay = new Dictionary<DateOnly, decimal>();
        var remaining = requestedUnits;

        for (var i = 0; i < overlapDays && remaining > 0; i++)
        {
            var day = overlapStart.AddDays(i);
            var allocation = Math.Min(1m, remaining);

            if (leave.IsHalfDay == true && requestedUnits <= 0.5m)
            {
                allocation = Math.Min(0.5m, remaining);
            }

            unitsByDay[day] = allocation;
            remaining -= allocation;
        }

        return unitsByDay;
    }

    private static (DateOnly Start, DateOnly End) GetOverlapRange(LeaveRequest leave, DateOnly periodStart, DateOnly periodEnd)
    {
        var overlapStart = leave.StartDate > periodStart ? leave.StartDate : periodStart;
        var overlapEnd = leave.EndDate < periodEnd ? leave.EndDate : periodEnd;

        return (overlapStart, overlapEnd);
    }

    private static void ReduceDayUnit(IDictionary<DateOnly, decimal> bucket, DateOnly day, decimal amount)
    {
        if (!bucket.TryGetValue(day, out var existing))
        {
            return;
        }

        var remaining = existing - amount;
        if (remaining <= 0)
        {
            bucket.Remove(day);
        }
        else
        {
            bucket[day] = remaining;
        }
    }

    private static IEnumerable<DateOnly> EnumerateDays(DateOnly start, DateOnly end)
    {
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private Task ApplyOvertimeEarningsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var baseHourlyRate = ctx.BasicSalary / (ctx.WorkingDaysPerMonth * ctx.WorkingHoursPerDay);

        foreach (var overtime in ctx.Overtime)
        {
            var multiplier = overtime.Type switch
            {
                OvertimeType.Weekend => ctx.WeekendOvertimeMultiplier,
                OvertimeType.PublicHoliday => ctx.HolidayOvertimeMultiplier,
                _ => ctx.WeekdayOvertimeMultiplier
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

            overtime.PayRunId = payRun.Id;
            overtime.IsLockedForPayroll = true;
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

    private Task ApplyRecurringRulesAsync(PaySlipCalculationContext ctx, PayRun payRun, HashSet<string> recurringKeys)
    {
        var periodKey = $"{DateOnly.FromDateTime(payRun.PeriodStart):yyyyMMdd}-{DateOnly.FromDateTime(payRun.PeriodEnd):yyyyMMdd}";

        foreach (var rule in ctx.RecurringRules)
        {
            var key = $"{rule.Id}:{ctx.Employee.Id}:{periodKey}";
            if (!recurringKeys.Add(key))
            {
                continue;
            }

            var amount = RoundCurrency(rule.Amount);
            if (amount <= 0)
            {
                continue;
            }

            if (rule.RuleType == RecurringRuleType.Allowance)
            {
                ctx.Earnings.Add(new EarningLine
                {
                    Id = Guid.NewGuid(),
                    PaySlipId = ctx.PaySlipId,
                    Code = rule.Code,
                    Description = rule.Name,
                    Amount = amount,
                    IsEpfApplicable = rule.IsEpfApplicable,
                    IsEtfApplicable = rule.IsEtfApplicable,
                    IsTaxable = rule.IsTaxable
                });
            }
            else
            {
                ctx.Deductions.Add(new DeductionLine
                {
                    Id = Guid.NewGuid(),
                    PaySlipId = ctx.PaySlipId,
                    Code = rule.Code,
                    Description = rule.Name,
                    Amount = amount,
                    IsPreTax = false,
                    IsPostTax = true
                });
            }
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

            var scheduledRepayment = loan.Repayments
                .Where(r => !r.IsPaid)
                .OrderBy(r => r.DueDate)
                .FirstOrDefault();

            var installment = scheduledRepayment?.Amount ?? loan.InstallmentAmount;

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
            if (scheduledRepayment is not null)
            {
                scheduledRepayment.IsPaid = true;
            }

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
            CompanyId = payRun.CompanyId,
            BranchId = payRun.BranchId,
            CostCenterId = payRun.CostCenterId,
            IsConsolidated = payRun.IsConsolidated,
            Status = payRun.Status,
            ExportStatus = payRun.ExportStatus,
            ExportedBank = payRun.ExportedBank,
            ExportedAt = payRun.ExportedAt,
            ExportDownloadedAt = payRun.ExportDownloadedAt,
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
            CompanyId = summary.CompanyId,
            BranchId = summary.BranchId,
            CostCenterId = summary.CostCenterId,
            IsConsolidated = summary.IsConsolidated,
            Status = summary.Status,
            ExportStatus = summary.ExportStatus,
            ExportedBank = summary.ExportedBank,
            ExportedAt = summary.ExportedAt,
            ExportDownloadedAt = summary.ExportDownloadedAt,
            IsLocked = summary.IsLocked,
            EmployeeCount = summary.EmployeeCount,
            TotalNetPay = summary.TotalNetPay,
            PaySlips = payRun.PaySlips.Select(MapToDto).ToList(),
            PreparedAt = payRun.PreparedAt,
            PreparedByUserName = payRun.PreparedByUserName,
            ApprovedAt = payRun.ApprovedAt,
            ApprovedByUserName = payRun.ApprovedByUserName,
            LockedAt = payRun.LockedAt,
            LockedByUserName = payRun.LockedByUserName,
            StatusHistory = payRun.Approvals
                .OrderByDescending(a => a.ActionedAt)
                .Select(MapToDto)
                .ToList()
        };
    }

    private static PayRunStatusHistoryDto MapToDto(PayRunApproval approval)
    {
        return new PayRunStatusHistoryDto
        {
            Id = approval.Id,
            FromStatus = approval.FromStatus,
            ToStatus = approval.ToStatus,
            ActorUserId = approval.ActorUserId,
            ActorUserName = approval.ActorUserName,
            Comment = approval.Comment,
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

    private IQueryable<Employee> ApplyScopeFilter(
        IQueryable<Employee> query,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId)
    {
        if (companyId.HasValue)
        {
            query = query.Where(e => e.CompanyId == companyId);
        }

        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId);
        }

        if (costCenterId.HasValue)
        {
            query = query.Where(e => e.CostCenterId == costCenterId);
        }

        return query;
    }

    private async Task<(Guid? CompanyId, Guid? BranchId, Guid? CostCenterId)> ValidateOrganizationScopeAsync(
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        Guid? validatedCompanyId = companyId;
        Guid? validatedBranchId = branchId;

        if (validatedCompanyId.HasValue)
        {
            var companyExists = await _dbContext.Companies.AsNoTracking()
                .AnyAsync(c => c.Id == validatedCompanyId.Value, cancellationToken);

            if (!companyExists)
            {
                throw new InvalidOperationException("Company scope was not found.");
            }
        }

        if (validatedBranchId.HasValue)
        {
            var branch = await _dbContext.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == validatedBranchId.Value, cancellationToken);

            if (branch is null)
            {
                throw new InvalidOperationException("Branch scope was not found.");
            }

            if (validatedCompanyId.HasValue && branch.CompanyId != validatedCompanyId.Value)
            {
                throw new InvalidOperationException("Branch does not belong to the specified company.");
            }

            validatedCompanyId ??= branch.CompanyId;
        }

        if (costCenterId.HasValue)
        {
            var costCenter = await _dbContext.CostCenters.AsNoTracking()
                .FirstOrDefaultAsync(cc => cc.Id == costCenterId.Value, cancellationToken);

            if (costCenter is null)
            {
                throw new InvalidOperationException("Cost center scope was not found.");
            }

            if (costCenter.BranchId.HasValue)
            {
                if (validatedBranchId.HasValue && costCenter.BranchId != validatedBranchId.Value)
                {
                    throw new InvalidOperationException("Cost center does not belong to the specified branch.");
                }

                validatedBranchId ??= costCenter.BranchId;
            }

            if (costCenter.CompanyId.HasValue)
            {
                if (validatedCompanyId.HasValue && costCenter.CompanyId != validatedCompanyId.Value)
                {
                    throw new InvalidOperationException("Cost center does not belong to the specified company.");
                }

                validatedCompanyId ??= costCenter.CompanyId;
            }
        }

        if (validatedBranchId.HasValue && !validatedCompanyId.HasValue)
        {
            validatedCompanyId = await _dbContext.Branches.AsNoTracking()
                .Where(b => b.Id == validatedBranchId.Value)
                .Select(b => b.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return (validatedCompanyId, validatedBranchId, costCenterId);
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
        public List<RecurringRule> RecurringRules { get; init; } = new();
        public List<LeaveRequest> LeaveRequests { get; init; } = new();
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
        public int WorkingDaysPerMonth { get; init; }
        public int WorkingHoursPerDay { get; init; }
        public decimal WeekdayOvertimeMultiplier { get; init; }
        public decimal WeekendOvertimeMultiplier { get; init; }
        public decimal HolidayOvertimeMultiplier { get; init; }
    }

    private sealed class PayrollSettingsSnapshot
    {
        public int WorkingDaysPerMonth { get; init; }
        public int WorkingHoursPerDay { get; init; }
        public decimal WeekdayOvertimeMultiplier { get; init; }
        public decimal WeekendOvertimeMultiplier { get; init; }
        public decimal HolidayOvertimeMultiplier { get; init; }
    }

    // TODO: Add integration tests to cover basic, overtime, and statutory calculation scenarios.
}
