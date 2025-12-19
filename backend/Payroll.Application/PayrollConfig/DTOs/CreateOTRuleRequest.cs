namespace Payroll.Application.PayrollConfig.DTOs;

public class CreateOTRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal WeekdayMultiplier { get; set; }
    public decimal WeekendMultiplier { get; set; }
    public decimal HolidayMultiplier { get; set; }
    public int RoundingMinutes { get; set; }
    public double DailyCapHours { get; set; }
    public double PayRunCapHours { get; set; }
    public bool AppliesOnWeekend { get; set; } = true;
    public bool AppliesOnHoliday { get; set; } = true;
}
