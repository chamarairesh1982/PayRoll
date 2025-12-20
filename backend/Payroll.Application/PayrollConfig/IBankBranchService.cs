using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public interface IBankBranchService
{
    Task<PaginatedResult<BankBranchDto>> GetAsync(int page, int pageSize, Guid? bankId, string? search, bool? isActive);
    Task<BankBranchDto?> GetByIdAsync(Guid id);
    Task<BankBranchDto> CreateAsync(CreateBankBranchRequest request);
    Task UpdateAsync(Guid id, UpdateBankBranchRequest request);
    Task DeleteAsync(Guid id);
}
