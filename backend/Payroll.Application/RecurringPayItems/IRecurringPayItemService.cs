using Payroll.Application.DTOs.RecurringPayItems;
using Payroll.Shared;

namespace Payroll.Application.RecurringPayItems;

public interface IRecurringPayItemService
{
    Task<PaginatedResult<RecurringPayItemRuleDto>> GetRulesAsync(int page, int pageSize, bool? isActive, CancellationToken cancellationToken = default);
    Task<RecurringPayItemRuleDto?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RecurringPayItemRuleDto> CreateRuleAsync(CreateRecurringPayItemRuleRequest request, CancellationToken cancellationToken = default);
    Task UpdateRuleAsync(Guid id, UpdateRecurringPayItemRuleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecurringPayItemAssignmentDto>> GetAssignmentsAsync(Guid? employeeId, Guid? ruleId, bool? isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringPayItemAssignmentDto>> CreateAssignmentsAsync(CreateRecurringPayItemAssignmentRequest request, CancellationToken cancellationToken = default);
    Task UpdateAssignmentAsync(Guid id, UpdateRecurringPayItemAssignmentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RecurringPayItemSimulationResponseDto> SimulateAsync(RecurringPayItemSimulationRequest request, CancellationToken cancellationToken = default);
}
