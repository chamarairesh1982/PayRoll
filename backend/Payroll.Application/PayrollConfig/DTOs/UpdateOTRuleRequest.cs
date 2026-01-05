namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateOTRuleRequest
{
    public Payroll.Domain.Overtime.OvertimeType Type { get; set; }
    public decimal Multiplier { get; set; }
    public int RoundToMinutes { get; set; }
    public Payroll.Domain.Overtime.OvertimeRoundingMode RoundingMode { get; set; } = Payroll.Domain.Overtime.OvertimeRoundingMode.Nearest;
    public double? DailyHoursCap { get; set; }
    public double? MonthlyHoursCap { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
