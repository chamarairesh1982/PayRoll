using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DashboardSummaryRequest request, CancellationToken cancellationToken = default);
}
