using Payroll.Application.Overtime.DTOs;
using Payroll.Domain.Overtime;
using Payroll.Shared;

namespace Payroll.Application.Overtime;

public interface IOvertimeService
{
    Task<PaginatedResult<OTEntryDto>> GetAsync(
        int page,
        int pageSize,
        Guid? employeeId,
        DateOnly? from,
        DateOnly? to,
        OvertimeStatus? status);

    Task<OTEntryDto?> GetByIdAsync(Guid id);

    Task<OTEntryDto> CreateAsync(CreateOTEntryRequest request);

    Task UpdateAsync(Guid id, UpdateOTEntryRequest request);

    Task SubmitAsync(Guid id);

    Task ApproveAsync(Guid id, OvertimeActionRequest request);

    Task RejectAsync(Guid id, OvertimeActionRequest request);
}
