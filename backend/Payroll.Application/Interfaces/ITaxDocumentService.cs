using Payroll.Application.DTOs;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Interfaces;

public interface ITaxDocumentService
{
    Task<TaxDocumentMetadataDto> GenerateMonthlyReportAsync(
        MonthlyTaxReportRequestDto request,
        bool regenerate,
        CancellationToken cancellationToken = default);

    Task<TaxDocumentMetadataDto> GenerateAnnualReportAsync(
        AnnualTaxReportRequestDto request,
        bool regenerate,
        CancellationToken cancellationToken = default);

    Task<TaxDocumentMetadataDto> GenerateEmployeeCertificateAsync(
        TaxCertificateRequestDto request,
        bool regenerate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaxDocumentHistoryDto>> GetDocumentsAsync(
        GeneratedTaxDocumentType? type = null,
        int? year = null,
        Guid? employeeId = null,
        CancellationToken cancellationToken = default);

    Task<FileExportResultDto?> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default);
}
