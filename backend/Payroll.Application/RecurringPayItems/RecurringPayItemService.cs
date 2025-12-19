using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs.RecurringPayItems;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.RecurringPayItems;

public class RecurringPayItemService : IRecurringPayItemService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RecurringPayItemService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<RecurringPayItemRuleDto>> GetRulesAsync(
        int page,
        int pageSize,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RecurringPayItemRules
            .AsNoTracking()
            .Include(r => r.AllowanceType)
            .Include(r => r.DeductionType)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<RecurringPayItemRuleDto>
        {
            Items = items.Select(MapRuleToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<RecurringPayItemRuleDto?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.RecurringPayItemRules
            .AsNoTracking()
            .Include(r => r.AllowanceType)
            .Include(r => r.DeductionType)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return rule is null ? null : MapRuleToDto(rule);
    }

    public async Task<RecurringPayItemRuleDto> CreateRuleAsync(
        CreateRecurringPayItemRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var (allowanceType, deductionType) = await ResolveComponentAsync(request.RuleType, request.PayComponentId, cancellationToken);

        var rule = new RecurringPayItemRule
        {
            Name = request.Name,
            RuleType = request.RuleType,
            AllowanceTypeId = allowanceType?.Id,
            DeductionTypeId = deductionType?.Id,
            Amount = request.Amount,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Taxable = request.Taxable,
            EpfEtfContributable = request.EpfEtfContributable,
            Prorate = request.Prorate,
            IsActive = request.IsActive,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.RecurringPayItemRules.AddAsync(rule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        rule.AllowanceType = allowanceType;
        rule.DeductionType = deductionType;

        return MapRuleToDto(rule);
    }

    public async Task UpdateRuleAsync(Guid id, UpdateRecurringPayItemRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.RecurringPayItemRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule is null)
        {
            throw new KeyNotFoundException("Recurring rule not found");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (rule.StartDate < today && request.StartDate <= rule.StartDate)
        {
            throw new InvalidOperationException(
                "Recurring rule updates for past-effective rules must use a future effective start date.");
        }

        if (rule.StartDate < today && request.StartDate > rule.StartDate)
        {
            rule.EndDate = request.StartDate.AddDays(-1);
            rule.ModifiedAt = DateTime.UtcNow;
            rule.ModifiedBy = _currentUserService.UserName ?? "system";

            var (allowanceType, deductionType) = await ResolveComponentAsync(request.RuleType, request.PayComponentId, cancellationToken);

            var newRule = new RecurringPayItemRule
            {
                Name = request.Name,
                RuleType = request.RuleType,
                AllowanceTypeId = allowanceType?.Id,
                DeductionTypeId = deductionType?.Id,
                Amount = request.Amount,
                Frequency = request.Frequency,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Taxable = request.Taxable,
                EpfEtfContributable = request.EpfEtfContributable,
                Prorate = request.Prorate,
                IsActive = request.IsActive,
                CreatedBy = _currentUserService.UserName ?? "system"
            };

            await _dbContext.RecurringPayItemRules.AddAsync(newRule, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var (updatedAllowance, updatedDeduction) = await ResolveComponentAsync(request.RuleType, request.PayComponentId, cancellationToken);

        rule.Name = request.Name;
        rule.RuleType = request.RuleType;
        rule.AllowanceTypeId = updatedAllowance?.Id;
        rule.DeductionTypeId = updatedDeduction?.Id;
        rule.Amount = request.Amount;
        rule.Frequency = request.Frequency;
        rule.StartDate = request.StartDate;
        rule.EndDate = request.EndDate;
        rule.Taxable = request.Taxable;
        rule.EpfEtfContributable = request.EpfEtfContributable;
        rule.Prorate = request.Prorate;
        rule.IsActive = request.IsActive;
        rule.ModifiedAt = DateTime.UtcNow;
        rule.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.RecurringPayItemRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule is null)
        {
            throw new KeyNotFoundException("Recurring rule not found");
        }

        rule.IsActive = false;
        rule.ModifiedAt = DateTime.UtcNow;
        rule.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringPayItemAssignmentDto>> GetAssignmentsAsync(
        Guid? employeeId,
        Guid? ruleId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RecurringPayItemAssignments
            .AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.Rule)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId);
        }

        if (ruleId.HasValue)
        {
            query = query.Where(a => a.RuleId == ruleId);
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(MapAssignmentToDto).ToList();
    }

    public async Task<IReadOnlyList<RecurringPayItemAssignmentDto>> CreateAssignmentsAsync(
        CreateRecurringPayItemAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.EmployeeIds.Count == 0)
        {
            return Array.Empty<RecurringPayItemAssignmentDto>();
        }

        var rule = await _dbContext.RecurringPayItemRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RuleId, cancellationToken);

        if (rule is null)
        {
            throw new KeyNotFoundException("Recurring rule not found");
        }

        var assignments = request.EmployeeIds
            .Distinct()
            .Select(employeeId => new RecurringPayItemAssignment
            {
                RuleId = request.RuleId,
                EmployeeId = employeeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                CreatedBy = _currentUserService.UserName ?? "system"
            })
            .ToList();

        await _dbContext.RecurringPayItemAssignments.AddRangeAsync(assignments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var storedAssignments = await _dbContext.RecurringPayItemAssignments
            .AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.Rule)
            .Where(a => assignments.Select(item => item.Id).Contains(a.Id))
            .ToListAsync(cancellationToken);

        return storedAssignments.Select(MapAssignmentToDto).ToList();
    }

    public async Task UpdateAssignmentAsync(
        Guid id,
        UpdateRecurringPayItemAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await _dbContext.RecurringPayItemAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (assignment is null)
        {
            throw new KeyNotFoundException("Recurring assignment not found");
        }

        assignment.StartDate = request.StartDate;
        assignment.EndDate = request.EndDate;
        assignment.IsActive = request.IsActive;
        assignment.ModifiedAt = DateTime.UtcNow;
        assignment.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _dbContext.RecurringPayItemAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (assignment is null)
        {
            throw new KeyNotFoundException("Recurring assignment not found");
        }

        assignment.IsActive = false;
        assignment.ModifiedAt = DateTime.UtcNow;
        assignment.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RecurringPayItemSimulationResponseDto> SimulateAsync(
        RecurringPayItemSimulationRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignments = await _dbContext.RecurringPayItemAssignments
            .AsNoTracking()
            .Include(a => a.Rule)
                .ThenInclude(r => r!.AllowanceType)
            .Include(a => a.Rule)
                .ThenInclude(r => r!.DeductionType)
            .Where(a => a.EmployeeId == request.EmployeeId
                        && a.IsActive
                        && a.StartDate <= request.PeriodEnd
                        && (a.EndDate == null || a.EndDate >= request.PeriodStart)
                        && a.Rule != null
                        && a.Rule.IsActive
                        && a.Rule.Frequency == PayPeriodType.Monthly
                        && a.Rule.StartDate <= request.PeriodEnd
                        && (a.Rule.EndDate == null || a.Rule.EndDate >= request.PeriodStart))
            .ToListAsync(cancellationToken);

        var items = new List<RecurringPayItemSimulationItemDto>();
        var totalAllowances = 0m;
        var totalDeductions = 0m;

        foreach (var assignment in assignments)
        {
            var rule = assignment.Rule!;
            var overlap = GetEffectiveRange(rule.StartDate, rule.EndDate, assignment.StartDate, assignment.EndDate, request.PeriodStart, request.PeriodEnd);
            if (overlap is null)
            {
                continue;
            }

            var (effectiveStart, effectiveEnd) = overlap.Value;
            var amount = CalculateRecurringAmount(rule, request.PeriodStart, request.PeriodEnd, effectiveStart, effectiveEnd);
            if (amount <= 0)
            {
                continue;
            }

            var (code, name) = ResolveComponentDisplay(rule);

            items.Add(new RecurringPayItemSimulationItemDto(
                rule.Id,
                assignment.Id,
                rule.Name,
                rule.RuleType,
                code,
                name,
                effectiveStart,
                effectiveEnd,
                amount,
                rule.Prorate && (effectiveStart > request.PeriodStart || effectiveEnd < request.PeriodEnd)));

            if (rule.RuleType == RecurringRuleType.Allowance)
            {
                totalAllowances += amount;
            }
            else
            {
                totalDeductions += amount;
            }
        }

        return new RecurringPayItemSimulationResponseDto(items, totalAllowances, totalDeductions);
    }

    private async Task<(AllowanceType? Allowance, DeductionType? Deduction)> ResolveComponentAsync(
        RecurringRuleType ruleType,
        Guid payComponentId,
        CancellationToken cancellationToken)
    {
        if (ruleType == RecurringRuleType.Allowance)
        {
            var allowance = await _dbContext.AllowanceTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == payComponentId, cancellationToken);

            if (allowance is null)
            {
                throw new KeyNotFoundException("Allowance type not found for recurring rule.");
            }

            return (allowance, null);
        }

        var deduction = await _dbContext.DeductionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == payComponentId, cancellationToken);

        if (deduction is null)
        {
            throw new KeyNotFoundException("Deduction type not found for recurring rule.");
        }

        return (null, deduction);
    }

    private static RecurringPayItemRuleDto MapRuleToDto(RecurringPayItemRule rule)
    {
        var (code, name, componentId) = ResolveComponentInfo(rule);

        return new RecurringPayItemRuleDto(
            rule.Id,
            rule.Name,
            rule.RuleType,
            componentId,
            code,
            name,
            rule.Frequency,
            rule.StartDate,
            rule.EndDate,
            rule.Amount,
            rule.Taxable,
            rule.EpfEtfContributable,
            rule.Prorate,
            rule.IsActive);
    }

    private static RecurringPayItemAssignmentDto MapAssignmentToDto(RecurringPayItemAssignment assignment)
    {
        var employeeName = assignment.Employee?.FullName ?? assignment.EmployeeId.ToString();
        var ruleName = assignment.Rule?.Name ?? assignment.RuleId.ToString();

        return new RecurringPayItemAssignmentDto(
            assignment.Id,
            assignment.RuleId,
            ruleName,
            assignment.EmployeeId,
            employeeName,
            assignment.StartDate,
            assignment.EndDate,
            assignment.IsActive);
    }

    private static (DateOnly Start, DateOnly End)? GetEffectiveRange(
        DateOnly ruleStart,
        DateOnly? ruleEnd,
        DateOnly assignmentStart,
        DateOnly? assignmentEnd,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var effectiveStart = new[] { ruleStart, assignmentStart, periodStart }.Max();
        var effectiveEnd = new[] { ruleEnd ?? DateOnly.MaxValue, assignmentEnd ?? DateOnly.MaxValue, periodEnd }.Min();

        if (effectiveEnd < effectiveStart)
        {
            return null;
        }

        return (effectiveStart, effectiveEnd);
    }

    private static decimal CalculateRecurringAmount(
        RecurringPayItemRule rule,
        DateOnly periodStart,
        DateOnly periodEnd,
        DateOnly effectiveStart,
        DateOnly effectiveEnd)
    {
        if (!rule.Prorate || (effectiveStart == periodStart && effectiveEnd == periodEnd))
        {
            return Math.Round(rule.Amount, 2, MidpointRounding.AwayFromZero);
        }

        var activeDays = effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
        var periodDays = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var prorated = rule.Amount * activeDays / periodDays;
        return Math.Round(prorated, 2, MidpointRounding.AwayFromZero);
    }

    private static (string Code, string Name, Guid ComponentId) ResolveComponentInfo(RecurringPayItemRule rule)
    {
        if (rule.RuleType == RecurringRuleType.Allowance)
        {
            var allowance = rule.AllowanceType ?? throw new InvalidOperationException("Allowance component missing.");
            return (allowance.Code, allowance.Name, allowance.Id);
        }

        var deduction = rule.DeductionType ?? throw new InvalidOperationException("Deduction component missing.");
        return (deduction.Code, deduction.Name, deduction.Id);
    }

    private static (string Code, string Name) ResolveComponentDisplay(RecurringPayItemRule rule)
    {
        if (rule.RuleType == RecurringRuleType.Allowance)
        {
            var allowance = rule.AllowanceType;
            return (allowance?.Code ?? string.Empty, allowance?.Name ?? string.Empty);
        }

        var deduction = rule.DeductionType;
        return (deduction?.Code ?? string.Empty, deduction?.Name ?? string.Empty);
    }
}
