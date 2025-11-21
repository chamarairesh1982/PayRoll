using Payroll.Application.DTOs;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface IAttendanceService
{
    Task<PaginatedResult<AttendanceDto>> GetAttendanceAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task RecordAttendanceAsync(AttendanceDto attendance, CancellationToken cancellationToken = default);
}
