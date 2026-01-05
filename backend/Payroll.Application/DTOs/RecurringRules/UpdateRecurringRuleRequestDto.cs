using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs.RecurringRules;

public record UpdateRecurringRuleRequestDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public RecurringRuleType RuleType { get; init; }
    public PayPeriodType Frequency { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal Amount { get; init; }
    public Guid EmployeeId { get; init; }
    public bool IsEpfApplicable { get; init; }
    public bool IsEtfApplicable { get; init; }
    public bool IsTaxable { get; init; }
    public bool IsActive { get; init; } = true;
}
