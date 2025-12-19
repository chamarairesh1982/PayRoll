namespace Payroll.Application.DTOs.RecurringPayItems;

public record CreateRecurringPayItemAssignmentRequest
{
    public Guid RuleId { get; init; }
    public List<Guid> EmployeeIds { get; init; } = new();
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateRecurringPayItemAssignmentRequest
{
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool IsActive { get; init; } = true;
}
