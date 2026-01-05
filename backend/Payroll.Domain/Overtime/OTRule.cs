using Payroll.Domain.Common;

namespace Payroll.Domain.Overtime;

public class OTRule : AuditableEntity
{
    public OvertimeType Type { get; set; }

    public decimal Multiplier { get; set; }

    public int RoundToMinutes { get; set; }

    public OvertimeRoundingMode RoundingMode { get; set; }

    public double? DailyHoursCap { get; set; }

    public double? MonthlyHoursCap { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }
}
