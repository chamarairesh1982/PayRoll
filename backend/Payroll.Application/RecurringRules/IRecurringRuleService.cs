using Payroll.Application.DTOs.RecurringRules;
using Payroll.Shared;

namespace Payroll.Application.RecurringRules;

public interface IRecurringRuleService
{
    Task<PaginatedResult<RecurringRuleDto>> GetAsync(int page, int pageSize, bool? isActive, CancellationToken cancellationToken = default);
    Task<RecurringRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RecurringRuleDto> CreateAsync(CreateRecurringRuleRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateRecurringRuleRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringRuleSimulationResultDto>> SimulateAsync(RecurringRuleSimulationRequestDto request, CancellationToken cancellationToken = default);
}
