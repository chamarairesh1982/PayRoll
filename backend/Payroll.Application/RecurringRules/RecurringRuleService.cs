using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs.RecurringRules;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;
using Payroll.Shared;

namespace Payroll.Application.RecurringRules;

public class RecurringRuleService : IRecurringRuleService
{
    private readonly IPayrollDbContext _dbContext;

    public RecurringRuleService(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<RecurringRuleDto>> GetAsync(int page, int pageSize, bool? isActive, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.RecurringRules
            .AsNoTracking()
            .Include(r => r.Employee)
            .OrderBy(r => r.Code)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedResult<RecurringRuleDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RecurringRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.RecurringRules
            .AsNoTracking()
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return rule is null ? null : MapToDto(rule);
    }

    public async Task<RecurringRuleDto> CreateAsync(CreateRecurringRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        var rule = new RecurringRule
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            RuleType = request.RuleType,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Amount = request.Amount,
            EmployeeId = request.EmployeeId,
            IsEpfApplicable = request.IsEpfApplicable,
            IsEtfApplicable = request.IsEtfApplicable,
            IsTaxable = request.IsTaxable,
            IsActive = request.IsActive,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.RecurringRules.AddAsync(rule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(rule);
    }

    public async Task UpdateAsync(Guid id, UpdateRecurringRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.RecurringRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is null)
        {
            throw new KeyNotFoundException("Recurring rule not found");
        }

        rule.Code = request.Code.Trim();
        rule.Name = request.Name.Trim();
        rule.RuleType = request.RuleType;
        rule.Frequency = request.Frequency;
        rule.StartDate = request.StartDate;
        rule.EndDate = request.EndDate;
        rule.Amount = request.Amount;
        rule.EmployeeId = request.EmployeeId;
        rule.IsEpfApplicable = request.IsEpfApplicable;
        rule.IsEtfApplicable = request.IsEtfApplicable;
        rule.IsTaxable = request.IsTaxable;
        rule.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.RecurringRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is null)
        {
            return;
        }

        _dbContext.RecurringRules.Remove(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<RecurringRuleSimulationResultDto>> SimulateAsync(RecurringRuleSimulationRequestDto request, CancellationToken cancellationToken = default)
    {
        var startDate = request.StartFrom ?? request.Rule.StartDate;
        var periods = Math.Max(1, request.Periods);
        var results = new List<RecurringRuleSimulationResultDto>();
        var currentStart = startDate;

        for (var i = 0; i < periods; i++)
        {
            var (periodStart, periodEnd) = ResolvePeriod(currentStart, request.Rule.Frequency);
            results.Add(new RecurringRuleSimulationResultDto(periodStart, periodEnd, request.Rule.Amount));
            currentStart = NextPeriodStart(periodStart, request.Rule.Frequency);
        }

        return Task.FromResult<IReadOnlyList<RecurringRuleSimulationResultDto>>(results);
    }

    private static RecurringRuleDto MapToDto(RecurringRule rule)
    {
        return new RecurringRuleDto(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.RuleType,
            rule.Frequency,
            rule.StartDate,
            rule.EndDate,
            rule.Amount,
            rule.EmployeeId,
            rule.Employee?.FullName,
            rule.IsEpfApplicable,
            rule.IsEtfApplicable,
            rule.IsTaxable,
            rule.IsActive);
    }

    private static (DateOnly Start, DateOnly End) ResolvePeriod(DateOnly start, PayPeriodType frequency)
    {
        return frequency switch
        {
            PayPeriodType.Weekly => (start, start.AddDays(6)),
            _ => (new DateOnly(start.Year, start.Month, 1), new DateOnly(start.Year, start.Month, 1).AddMonths(1).AddDays(-1))
        };
    }

    private static DateOnly NextPeriodStart(DateOnly currentStart, PayPeriodType frequency)
    {
        return frequency switch
        {
            PayPeriodType.Weekly => currentStart.AddDays(7),
            _ => currentStart.AddMonths(1)
        };
    }
}
