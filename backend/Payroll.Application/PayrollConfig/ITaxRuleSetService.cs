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
}
