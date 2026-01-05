using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public interface IDeductionTypeService
{
    Task<PaginatedResult<DeductionTypeDto>> GetAsync(int page, int pageSize, bool? isActive);
    Task<DeductionTypeDto?> GetByIdAsync(Guid id);
    Task<DeductionTypeDto> CreateAsync(CreateDeductionTypeRequest request);
    Task UpdateAsync(Guid id, UpdateDeductionTypeRequest request);
    Task DeleteAsync(Guid id);
}
