using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs.RecurringRules;

public record RecurringRuleDto(
    Guid Id,
    string Code,
    string Name,
    RecurringRuleType RuleType,
    PayPeriodType Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal Amount,
    Guid EmployeeId,
    string? EmployeeName,
    bool IsEpfApplicable,
    bool IsEtfApplicable,
    bool IsTaxable,
    bool IsActive);
