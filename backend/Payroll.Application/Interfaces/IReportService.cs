namespace Payroll.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateStatutoryReportAsync(
        DateOnly period,
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? costCenterId = null,
        bool? isConsolidated = null,
        CancellationToken cancellationToken = default);
}
