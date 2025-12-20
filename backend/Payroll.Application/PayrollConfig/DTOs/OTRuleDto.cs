namespace Payroll.Application.PayrollConfig.DTOs;

public record OTRuleDto(
    Guid Id,
    Payroll.Domain.Overtime.OvertimeType Type,
    decimal Multiplier,
    int RoundToMinutes,
    Payroll.Domain.Overtime.OvertimeRoundingMode RoundingMode,
    double? DailyHoursCap,
    double? MonthlyHoursCap,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive);
