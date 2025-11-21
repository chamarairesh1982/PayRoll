namespace Payroll.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateStatutoryReportAsync(DateOnly period, CancellationToken cancellationToken = default);
}
