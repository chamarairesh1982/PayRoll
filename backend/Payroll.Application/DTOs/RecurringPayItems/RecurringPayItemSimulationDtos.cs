using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs.RecurringPayItems;

public record RecurringPayItemSimulationRequest
{
    public Guid EmployeeId { get; init; }
    public Guid? PayPeriodId { get; init; }
    public DateOnly PeriodStart { get; init; }
    public DateOnly PeriodEnd { get; init; }
}

public record RecurringPayItemSimulationItemDto(
    Guid RuleId,
    Guid AssignmentId,
    string Name,
    RecurringRuleType RuleType,
    string PayComponentCode,
    string PayComponentName,
    DateOnly EffectiveStart,
    DateOnly EffectiveEnd,
    decimal Amount,
    bool Prorated);

public record RecurringPayItemSimulationResponseDto(
    IReadOnlyList<RecurringPayItemSimulationItemDto> Items,
    decimal TotalAllowances,
    decimal TotalDeductions);
