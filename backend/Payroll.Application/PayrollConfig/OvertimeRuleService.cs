using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.Overtime;

namespace Payroll.Application.PayrollConfig;

public class OvertimeRuleService : IOvertimeRuleService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public OvertimeRuleService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<OTRuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rules = await _dbContext.OTRules
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Type)
            .ThenBy(r => r.EffectiveFrom)
            .ToListAsync(ct);

        return rules.Select(MapToDto).ToList();
    }

    public async Task<OTRuleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _dbContext.OTRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

        return rule is null ? null : MapToDto(rule);
    }

    public async Task<OTRuleDto> CreateAsync(CreateOTRuleRequest request, CancellationToken ct = default)
    {
        ValidateRule(request.Type, request.Multiplier, request.RoundToMinutes, request.DailyHoursCap, request.MonthlyHoursCap, request.EffectiveFrom, request.EffectiveTo);
        await EnsureNoOverlapAsync(request.Type, request.EffectiveFrom, request.EffectiveTo, null, request.IsActive, ct);

        var rule = new OTRule
        {
            Type = request.Type,
            Multiplier = request.Multiplier,
            RoundToMinutes = request.RoundToMinutes,
            RoundingMode = request.RoundingMode,
            DailyHoursCap = request.DailyHoursCap,
            MonthlyHoursCap = request.MonthlyHoursCap,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.OTRules.AddAsync(rule, ct);
        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(rule);
    }

    public async Task<OTRuleDto> UpdateAsync(Guid id, UpdateOTRuleRequest request, CancellationToken ct = default)
    {
        var rule = await _dbContext.OTRules.FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

        if (rule is null)
        {
            throw new KeyNotFoundException("Overtime rule not found");
        }

        ValidateRule(request.Type, request.Multiplier, request.RoundToMinutes, request.DailyHoursCap, request.MonthlyHoursCap, request.EffectiveFrom, request.EffectiveTo);
        await EnsureNoOverlapAsync(request.Type, request.EffectiveFrom, request.EffectiveTo, rule.Id, request.IsActive, ct);

        rule.Type = request.Type;
        rule.Multiplier = request.Multiplier;
        rule.RoundToMinutes = request.RoundToMinutes;
        rule.RoundingMode = request.RoundingMode;
        rule.DailyHoursCap = request.DailyHoursCap;
        rule.MonthlyHoursCap = request.MonthlyHoursCap;
        rule.EffectiveFrom = request.EffectiveFrom;
        rule.EffectiveTo = request.EffectiveTo;
        rule.IsActive = request.IsActive;
        rule.ModifiedAt = DateTime.UtcNow;
        rule.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(rule);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _dbContext.OTRules.FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

        if (rule is null)
        {
            throw new KeyNotFoundException("Overtime rule not found");
        }

        rule.IsActive = false;
        rule.ModifiedAt = DateTime.UtcNow;
        rule.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(ct);
    }

    private static OTRuleDto MapToDto(OTRule rule)
    {
        return new OTRuleDto(
            rule.Id,
            rule.Type,
            rule.Multiplier,
            rule.RoundToMinutes,
            rule.RoundingMode,
            rule.DailyHoursCap,
            rule.MonthlyHoursCap,
            rule.EffectiveFrom,
            rule.EffectiveTo,
            rule.IsActive);
    }

    private static void ValidateRule(
        OvertimeType type,
        decimal multiplier,
        int roundToMinutes,
        double? dailyHoursCap,
        double? monthlyHoursCap,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        if (!Enum.IsDefined(typeof(OvertimeType), type))
        {
            throw new ArgumentException("Invalid overtime type.");
        }

        if (multiplier <= 0)
        {
            throw new ArgumentException("Multiplier must be greater than zero.");
        }

        var allowedMinutes = new[] { 1, 5, 10, 15, 30, 60 };
        if (!allowedMinutes.Contains(roundToMinutes))
        {
            throw new ArgumentException("Round-to minutes must be one of 1, 5, 10, 15, 30, or 60.");
        }

        if (dailyHoursCap.HasValue && dailyHoursCap.Value < 0)
        {
            throw new ArgumentException("Daily hours cap must be greater than or equal to zero.");
        }

        if (monthlyHoursCap.HasValue && monthlyHoursCap.Value < 0)
        {
            throw new ArgumentException("Monthly hours cap must be greater than or equal to zero.");
        }

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new ArgumentException("Effective to date must be on or after effective from.");
        }
    }

    private async Task EnsureNoOverlapAsync(
        OvertimeType type,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? ruleId,
        bool isActive,
        CancellationToken ct)
    {
        if (!isActive)
        {
            return;
        }

        var query = _dbContext.OTRules.AsNoTracking().Where(r => r.IsActive && r.Type == type);
        if (ruleId.HasValue)
        {
            query = query.Where(r => r.Id != ruleId.Value);
        }

        var overlaps = await query.AnyAsync(r =>
            effectiveFrom <= (r.EffectiveTo ?? DateOnly.MaxValue)
            && (effectiveTo ?? DateOnly.MaxValue) >= r.EffectiveFrom,
            ct);

        if (overlaps)
        {
            throw new InvalidOperationException("Overlapping overtime rules are not allowed for the same overtime type.");
        }
    }
}
