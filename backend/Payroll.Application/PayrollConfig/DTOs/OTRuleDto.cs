namespace Payroll.Application.PayrollConfig.DTOs;

public record OTRuleDto(
    Guid Id,
    string Name,
    decimal WeekdayMultiplier,
    decimal WeekendMultiplier,
    decimal HolidayMultiplier,
    int RoundingMinutes,
    double DailyCapHours,
    double PayRunCapHours,
    bool AppliesOnWeekend,
    bool AppliesOnHoliday,
    bool IsActive);
