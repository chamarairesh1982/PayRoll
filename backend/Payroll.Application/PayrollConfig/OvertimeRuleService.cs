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
            .OrderBy(r => r.Name)
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
        var rule = new OTRule
        {
            Name = request.Name.Trim(),
            WeekdayMultiplier = Math.Max(0, request.WeekdayMultiplier),
            WeekendMultiplier = Math.Max(0, request.WeekendMultiplier),
            HolidayMultiplier = Math.Max(0, request.HolidayMultiplier),
            RoundingMinutes = Math.Max(0, request.RoundingMinutes),
            DailyCapHours = Math.Max(0, request.DailyCapHours),
            PayRunCapHours = Math.Max(0, request.PayRunCapHours),
            AppliesOnWeekend = request.AppliesOnWeekend,
            AppliesOnHoliday = request.AppliesOnHoliday,
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

        rule.Name = request.Name.Trim();
        rule.WeekdayMultiplier = Math.Max(0, request.WeekdayMultiplier);
        rule.WeekendMultiplier = Math.Max(0, request.WeekendMultiplier);
        rule.HolidayMultiplier = Math.Max(0, request.HolidayMultiplier);
        rule.RoundingMinutes = Math.Max(0, request.RoundingMinutes);
        rule.DailyCapHours = Math.Max(0, request.DailyCapHours);
        rule.PayRunCapHours = Math.Max(0, request.PayRunCapHours);
        rule.AppliesOnWeekend = request.AppliesOnWeekend;
        rule.AppliesOnHoliday = request.AppliesOnHoliday;
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
            rule.Name,
            rule.WeekdayMultiplier,
            rule.WeekendMultiplier,
            rule.HolidayMultiplier,
            rule.RoundingMinutes,
            rule.DailyCapHours,
            rule.PayRunCapHours,
            rule.AppliesOnWeekend,
            rule.AppliesOnHoliday,
            rule.IsActive);
    }
}
