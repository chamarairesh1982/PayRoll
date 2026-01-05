using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Shared;
using System;

namespace Payroll.Application.PayrollConfig;

public interface IEpfEtfRuleSetService
{
    Task<PaginatedResult<EpfEtfRuleSetDto>> GetAsync(int page, int pageSize, bool? isActive);
    Task<EpfEtfRuleSetDto?> GetByIdAsync(Guid id);
    Task<EpfEtfRuleSetDto> CreateAsync(CreateEpfEtfRuleSetRequest request);
    Task UpdateAsync(Guid id, UpdateEpfEtfRuleSetRequest request);
    Task<EpfEtfRuleSetDto?> GetActiveRuleForDateAsync(DateOnly date);
}
