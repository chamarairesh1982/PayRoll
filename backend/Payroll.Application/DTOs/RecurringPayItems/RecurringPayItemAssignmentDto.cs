namespace Payroll.Application.DTOs.RecurringPayItems;

public record RecurringPayItemAssignmentDto(
    Guid Id,
    Guid RuleId,
    string RuleName,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive);
