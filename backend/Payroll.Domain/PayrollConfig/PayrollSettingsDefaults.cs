namespace Payroll.Domain.PayrollConfig;

public static class PayrollSettingsDefaults
{
    public const int WorkingDaysPerMonth = 26;
    public const int WorkingHoursPerDay = 8;

    public const decimal WeekdayOvertimeMultiplier = 1.5m;
    public const decimal WeekendOvertimeMultiplier = 2.0m;
    public const decimal HolidayOvertimeMultiplier = 2.0m;

    public const int OvertimeRoundingMinutes = 15;
    public const double OvertimeDailyCapHours = 12.0;
    public const double OvertimePayRunCapHours = 80.0;
}
