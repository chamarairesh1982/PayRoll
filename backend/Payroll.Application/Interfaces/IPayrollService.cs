using Payroll.Application.DTOs;
using Payroll.Domain.Payroll;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface IPayrollService
{
    Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(int page, int pageSize, PayRunStatus? status = null, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto?> GetPayRunAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request, CancellationToken cancellationToken = default);
    Task RecalculatePayRunAsync(Guid id, RecalculatePayRunRequest request, CancellationToken cancellationToken = default);
    Task ChangeStatusAsync(Guid id, ChangePayRunStatusRequest request, CancellationToken cancellationToken = default);
    Task<PaySlipDto?> GetPaySlipAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default);
}
