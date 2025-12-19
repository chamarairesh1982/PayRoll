using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs.RecurringPayItems;

public record RecurringPayItemRuleDto(
    Guid Id,
    string Name,
    RecurringRuleType RuleType,
    Guid PayComponentId,
    string PayComponentCode,
    string PayComponentName,
    PayPeriodType Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal Amount,
    bool Taxable,
    bool EpfEtfContributable,
    bool Prorate,
    bool IsActive);
