using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class LeaveService : ILeaveService
{
    public Task ApproveLeaveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: approve leave
        return Task.CompletedTask;
    }

    public Task<LeaveDto> CreateLeaveRequestAsync(LeaveDto leave, CancellationToken cancellationToken = default)
    {
        // TODO: create leave request
        return Task.FromResult(leave);
    }

    public Task<LeaveDto?> GetLeaveRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: load leave request
        return Task.FromResult<LeaveDto?>(null);
    }

    public Task<PaginatedResult<LeaveDto>> GetLeaveRequestsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = new PaginatedResult<LeaveDto>
        {
            Items = Array.Empty<LeaveDto>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = 0
        };
        return Task.FromResult(result);
    }

    public Task RejectLeaveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: reject leave
        return Task.CompletedTask;
    }
}
