using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class PayrollSettings : AuditableEntity
{
    public int WorkingDaysPerMonth { get; set; } = PayrollSettingsDefaults.WorkingDaysPerMonth;

    public int WorkingHoursPerDay { get; set; } = PayrollSettingsDefaults.WorkingHoursPerDay;

    public CalculationBasis NoPayCalculationBasis { get; set; } = PayrollSettingsDefaults.NoPayCalculationBasis;

    public decimal AttendanceHalfDayHours { get; set; } = PayrollSettingsDefaults.AttendanceHalfDayHours;

    public decimal WeekdayOvertimeMultiplier { get; set; } = PayrollSettingsDefaults.WeekdayOvertimeMultiplier;

    public decimal WeekendOvertimeMultiplier { get; set; } = PayrollSettingsDefaults.WeekendOvertimeMultiplier;

    public decimal HolidayOvertimeMultiplier { get; set; } = PayrollSettingsDefaults.HolidayOvertimeMultiplier;

    public int OvertimeRoundingMinutes { get; set; } = PayrollSettingsDefaults.OvertimeRoundingMinutes;

    public double OvertimeDailyCapHours { get; set; } = PayrollSettingsDefaults.OvertimeDailyCapHours;

    public double OvertimePayRunCapHours { get; set; } = PayrollSettingsDefaults.OvertimePayRunCapHours;
}
