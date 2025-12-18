namespace Payroll.Application.PayrollConfig.DTOs;

public record OvertimeRuleConfigDto(
    decimal WeekdayOvertimeMultiplier,
    decimal WeekendOvertimeMultiplier,
    decimal HolidayOvertimeMultiplier,
    int OvertimeRoundingMinutes,
    double OvertimeDailyCapHours,
    double OvertimePayRunCapHours);
