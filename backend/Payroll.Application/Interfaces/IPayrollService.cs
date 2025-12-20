using Payroll.Application.DTOs;
using Payroll.Domain.Payroll;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface IPayrollService
{
    Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(PayRunQuery query, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto?> GetPayRunAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request, CancellationToken cancellationToken = default);
    Task RecalculatePayRunAsync(Guid id, RecalculatePayRunRequest request, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto> PreparePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default);
    Task ChangeStatusAsync(Guid id, ChangePayRunStatusRequest request, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto> ApprovePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default);
    Task<PayRunDetailDto> LockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default);
    Task UnlockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default);
    Task<PaySlipDto?> GetPaySlipAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default);
    Task<TaxCalculationSummaryDto> PreviewTaxAsync(TaxPreviewRequest request, CancellationToken cancellationToken = default);
    Task<GeneralLedgerExportDto> GenerateGeneralLedgerExportAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task ReviewGeneralLedgerExportAsync(Guid payRunId, GeneralLedgerActionRequest request, CancellationToken cancellationToken = default);
    Task ApproveGeneralLedgerExportAsync(Guid payRunId, GeneralLedgerActionRequest request, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> ExportGeneralLedgerAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<List<GeneralLedgerAccountMappingDto>> GetGeneralLedgerAccountMappingsAsync(CancellationToken cancellationToken = default);
    Task<GeneralLedgerAccountMappingDto> UpsertGeneralLedgerAccountMappingAsync(UpsertGeneralLedgerAccountMappingRequest request, CancellationToken cancellationToken = default);
    Task<ApitReportDto?> GetApitReportAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> GenerateApitCertificateAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> ExportPaySlipAsync(Guid payRunId, Guid paySlipId, string format = "pdf", CancellationToken cancellationToken = default);
}
