using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.PayrollConfig;

public interface IOvertimeRuleService
{
    Task<IReadOnlyList<OTRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<OTRuleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OTRuleDto> CreateAsync(CreateOTRuleRequest request, CancellationToken ct = default);
    Task<OTRuleDto> UpdateAsync(Guid id, UpdateOTRuleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
