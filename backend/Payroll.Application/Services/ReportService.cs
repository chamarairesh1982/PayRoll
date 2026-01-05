using Payroll.Application.Interfaces;

namespace Payroll.Application.Services;

public class ReportService : IReportService
{
    public Task<byte[]> GenerateStatutoryReportAsync(
        DateOnly period,
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? costCenterId = null,
        bool? isConsolidated = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: compile report output
        return Task.FromResult(Array.Empty<byte>());
    }
}
