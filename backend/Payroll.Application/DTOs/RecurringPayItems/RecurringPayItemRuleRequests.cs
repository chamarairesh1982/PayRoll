using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs.RecurringPayItems;

public record CreateRecurringPayItemRuleRequest
{
    public string Name { get; init; } = string.Empty;
    public RecurringRuleType RuleType { get; init; }
    public Guid PayComponentId { get; init; }
    public decimal Amount { get; init; }
    public PayPeriodType Frequency { get; init; } = PayPeriodType.Monthly;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool Taxable { get; init; }
    public bool EpfEtfContributable { get; init; }
    public bool Prorate { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateRecurringPayItemRuleRequest
{
    public string Name { get; init; } = string.Empty;
    public RecurringRuleType RuleType { get; init; }
    public Guid PayComponentId { get; init; }
    public decimal Amount { get; init; }
    public PayPeriodType Frequency { get; init; } = PayPeriodType.Monthly;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool Taxable { get; init; }
    public bool EpfEtfContributable { get; init; }
    public bool Prorate { get; init; }
    public bool IsActive { get; init; } = true;
}
