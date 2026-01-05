namespace Payroll.Application.DTOs.RecurringRules;

public record RecurringRuleSimulationResultDto(DateOnly PeriodStart, DateOnly PeriodEnd, decimal Amount);
