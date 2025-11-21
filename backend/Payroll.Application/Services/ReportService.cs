using Payroll.Application.Interfaces;

namespace Payroll.Application.Services;

public class ReportService : IReportService
{
    public Task<byte[]> GenerateStatutoryReportAsync(DateOnly period, CancellationToken cancellationToken = default)
    {
        // TODO: compile report output
        return Task.FromResult(Array.Empty<byte>());
    }
}
