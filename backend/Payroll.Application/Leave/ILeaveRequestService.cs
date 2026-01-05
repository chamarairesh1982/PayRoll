using Payroll.Application.Leave.DTOs;
using Payroll.Domain.Leave;
using Payroll.Shared;

namespace Payroll.Application.Leave;

public interface ILeaveRequestService
{
    Task<PaginatedResult<LeaveRequestDto>> GetAsync(int page, int pageSize, Guid? employeeId, LeaveStatus? status);

    Task<LeaveRequestDto?> GetByIdAsync(Guid id);

    Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestRequest request);

    Task UpdateAsync(Guid id, UpdateLeaveRequestRequest request);

    Task DeleteAsync(Guid id);
}
