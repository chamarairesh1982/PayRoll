using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class AttendanceService : IAttendanceService
{
    public Task<PaginatedResult<AttendanceDto>> GetAttendanceAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = new PaginatedResult<AttendanceDto>
        {
            Items = Array.Empty<AttendanceDto>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = 0
        };
        return Task.FromResult(result);
    }

    public Task RecordAttendanceAsync(AttendanceDto attendance, CancellationToken cancellationToken = default)
    {
        // TODO: persist attendance
        return Task.CompletedTask;
    }
}
