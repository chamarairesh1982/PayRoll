using Payroll.Application.Overtime.DTOs;
using Payroll.Domain.Overtime;
using Payroll.Shared;

namespace Payroll.Application.Overtime;

public interface IOvertimeService
{
    Task<PaginatedResult<OvertimeRecordDto>> GetAsync(
        int page,
        int pageSize,
        Guid? employeeId,
        DateOnly? date,
        OvertimeStatus? status);

    Task<OvertimeRecordDto?> GetByIdAsync(Guid id);

    Task<OvertimeRecordDto> CreateAsync(CreateOvertimeRecordRequest request);

    Task UpdateAsync(Guid id, UpdateOvertimeRecordRequest request);

    Task DeleteAsync(Guid id);
}
