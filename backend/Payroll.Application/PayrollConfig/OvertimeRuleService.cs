using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;

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

    public async Task<OvertimeRuleConfigDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await _dbContext.PayrollSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        return MapToDto(settings);
    }

    public async Task<OvertimeRuleConfigDto> UpdateAsync(UpdateOvertimeRuleConfigRequest request, CancellationToken ct = default)
    {
        var settings = await _dbContext.PayrollSettings.FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            settings = new PayrollSettings
            {
                WorkingDaysPerMonth = PayrollSettingsDefaults.WorkingDaysPerMonth,
                WorkingHoursPerDay = PayrollSettingsDefaults.WorkingHoursPerDay,
                WeekdayOvertimeMultiplier = PayrollSettingsDefaults.WeekdayOvertimeMultiplier,
                WeekendOvertimeMultiplier = PayrollSettingsDefaults.WeekendOvertimeMultiplier,
                HolidayOvertimeMultiplier = PayrollSettingsDefaults.HolidayOvertimeMultiplier,
                OvertimeRoundingMinutes = PayrollSettingsDefaults.OvertimeRoundingMinutes,
                OvertimeDailyCapHours = PayrollSettingsDefaults.OvertimeDailyCapHours,
                OvertimePayRunCapHours = PayrollSettingsDefaults.OvertimePayRunCapHours,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId ?? "system"
            };

            await _dbContext.PayrollSettings.AddAsync(settings, ct);
        }

        settings.WeekdayOvertimeMultiplier = request.WeekdayOvertimeMultiplier;
        settings.WeekendOvertimeMultiplier = request.WeekendOvertimeMultiplier;
        settings.HolidayOvertimeMultiplier = request.HolidayOvertimeMultiplier;
        settings.OvertimeRoundingMinutes = Math.Max(0, request.OvertimeRoundingMinutes);
        settings.OvertimeDailyCapHours = Math.Max(0, request.OvertimeDailyCapHours);
        settings.OvertimePayRunCapHours = Math.Max(0, request.OvertimePayRunCapHours);
        settings.ModifiedAt = DateTime.UtcNow;
        settings.ModifiedBy = _currentUserService.UserId ?? "system";

        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(settings);
    }

    private static OvertimeRuleConfigDto MapToDto(PayrollSettings? settings)
    {
        return new OvertimeRuleConfigDto(
            settings?.WeekdayOvertimeMultiplier ?? PayrollSettingsDefaults.WeekdayOvertimeMultiplier,
            settings?.WeekendOvertimeMultiplier ?? PayrollSettingsDefaults.WeekendOvertimeMultiplier,
            settings?.HolidayOvertimeMultiplier ?? PayrollSettingsDefaults.HolidayOvertimeMultiplier,
            settings?.OvertimeRoundingMinutes ?? PayrollSettingsDefaults.OvertimeRoundingMinutes,
            settings?.OvertimeDailyCapHours ?? PayrollSettingsDefaults.OvertimeDailyCapHours,
            settings?.OvertimePayRunCapHours ?? PayrollSettingsDefaults.OvertimePayRunCapHours);
    }
}
