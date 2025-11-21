using Payroll.Application.Payroll.DTOs;
using Payroll.Application.Payroll.Requests;
using Payroll.Domain.Payroll;
using Payroll.Shared;

namespace Payroll.Application.Payroll;

public interface IPayRunService
{
    Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(int page, int pageSize, PayRunStatus? status);
    Task<PayRunDetailDto?> GetPayRunAsync(Guid id);

    Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request);

    Task RecalculatePayRunAsync(Guid payRunId, RecalculatePayRunRequest request);

    Task ChangeStatusAsync(Guid payRunId, ChangePayRunStatusRequest request);

    Task<PaySlipDto?> GetPaySlipAsync(Guid payRunId, Guid paySlipId);
}
