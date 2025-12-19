using Payroll.Application.DTOs;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Interfaces;

public interface IStatutoryReportService
{
    Task<EpfEtfReportResultDto> GenerateEpfEtfReportAsync(EpfEtfReportRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatutoryReportHistoryDto>> GetReportsAsync(
        StatutoryReportType? type = null,
        Guid? payRunId = null,
        CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> DownloadReportAsync(
        Guid reportId,
        bool includeWarnings = false,
        CancellationToken cancellationToken = default);
}
