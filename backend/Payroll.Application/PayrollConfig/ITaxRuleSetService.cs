using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Shared;
using System;

namespace Payroll.Application.PayrollConfig;

public interface ITaxRuleSetService
{
    Task<PaginatedResult<TaxRuleSetDto>> GetAsync(int page, int pageSize, int? yearOfAssessment);
    Task<TaxRuleSetDto?> GetByIdAsync(Guid id);
    Task<TaxRuleSetDto> CreateAsync(CreateTaxRuleSetRequest request);
    Task UpdateAsync(Guid id, UpdateTaxRuleSetRequest request);
    Task<TaxRuleSetDto?> GetActiveRuleForDateAsync(DateOnly payDate);
    Task<IReadOnlyDictionary<Guid, TaxRuleSetDto>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<IReadOnlyList<TaxSlabDto>> GetSlabsAsync(Guid slabSetId);
    Task<TaxSlabDto> AddSlabAsync(Guid slabSetId, CreateTaxSlabRequest request);
    Task UpdateSlabAsync(Guid slabId, UpdateTaxSlabRequest request);
    Task DeleteSlabAsync(Guid slabId);
    Task<IReadOnlyList<TaxReliefDto>> GetReliefsAsync(Guid slabSetId);
    Task<TaxReliefDto> AddReliefAsync(Guid slabSetId, CreateTaxReliefRequest request);
    Task UpdateReliefAsync(Guid reliefId, UpdateTaxReliefRequest request);
    Task DeleteReliefAsync(Guid reliefId);
}
