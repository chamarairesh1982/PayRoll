using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IPayslipDocumentService
{
    Task<PayslipDocumentDto?> GenerateAsync(Guid payRunId, Guid employeeId, bool regenerate, CancellationToken cancellationToken = default);
    Task<PayslipBulkGenerateResultDto> GenerateBulkAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayslipDocumentDto>> GetDocumentsForPayRunAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default);
}
