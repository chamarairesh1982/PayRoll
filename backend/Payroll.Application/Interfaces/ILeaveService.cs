using Payroll.Application.DTOs;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface ILeaveService
{
    Task<PaginatedResult<LeaveDto>> GetLeaveRequestsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<LeaveDto?> GetLeaveRequestAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LeaveDto> CreateLeaveRequestAsync(LeaveDto leave, CancellationToken cancellationToken = default);
    Task ApproveLeaveAsync(Guid id, CancellationToken cancellationToken = default);
    Task RejectLeaveAsync(Guid id, CancellationToken cancellationToken = default);
}
