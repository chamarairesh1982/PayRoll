using Payroll.Application.DTOs;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface IPayrollService
{
    Task<PaginatedResult<PayRunDto>> GetPayRunsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PayRunDto?> GetPayRunAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PayRunDto> CreatePayRunAsync(PayRunDto payRun, CancellationToken cancellationToken = default);
    Task<PayRunDto> RecalculatePayRunAsync(Guid id, CancellationToken cancellationToken = default);
    Task ApprovePayRunAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaySlipDto?> GetPaySlipAsync(Guid id, CancellationToken cancellationToken = default);
}
