using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs.RecurringRules;

public record RecurringRuleSimulationRequestDto
{
    public CreateRecurringRuleRequestDto Rule { get; init; } = new();
    public int Periods { get; init; } = 1;
    public DateOnly? StartFrom { get; init; }
}
