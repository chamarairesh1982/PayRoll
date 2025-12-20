using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public interface IBankService
{
    Task<PaginatedResult<BankDto>> GetAsync(int page, int pageSize, string? search, bool? isActive);
    Task<BankDto?> GetByIdAsync(Guid id);
    Task<BankDto> CreateAsync(CreateBankRequest request);
    Task UpdateAsync(Guid id, UpdateBankRequest request);
    Task DeleteAsync(Guid id);
}
