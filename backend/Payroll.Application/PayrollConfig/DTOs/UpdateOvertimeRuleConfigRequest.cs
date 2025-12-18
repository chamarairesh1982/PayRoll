namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateOvertimeRuleConfigRequest
{
    public decimal WeekdayOvertimeMultiplier { get; set; }

    public decimal WeekendOvertimeMultiplier { get; set; }

    public decimal HolidayOvertimeMultiplier { get; set; }

    public int OvertimeRoundingMinutes { get; set; }

    public double OvertimeDailyCapHours { get; set; }

    public double OvertimePayRunCapHours { get; set; }
}
