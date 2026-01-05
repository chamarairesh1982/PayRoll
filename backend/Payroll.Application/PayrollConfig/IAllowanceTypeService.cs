using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public interface IAllowanceTypeService
{
    Task<PaginatedResult<AllowanceTypeDto>> GetAsync(int page, int pageSize, bool? isActive);
    Task<AllowanceTypeDto?> GetByIdAsync(Guid id);
    Task<AllowanceTypeDto> CreateAsync(CreateAllowanceTypeRequest request);
    Task UpdateAsync(Guid id, UpdateAllowanceTypeRequest request);
    Task DeleteAsync(Guid id);
}
