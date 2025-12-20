using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IBankExportService
{
    Task<IReadOnlyList<BankExportTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayRunBankExportDto>> GetExportsAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<BankExportGenerateResultDto> GenerateExportAsync(
        Guid payRunId,
        BankExportGenerateRequest request,
        bool regenerate,
        CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> DownloadExportAsync(Guid exportId, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> DownloadErrorsAsync(Guid exportId, CancellationToken cancellationToken = default);
}
