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
    Task<TimeReconciliationResultDto?> GetTimeReconciliationAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<TaxCalculationSummaryDto> PreviewTaxAsync(TaxPreviewRequest request, CancellationToken cancellationToken = default);
    Task<GlJournalBatchDetailDto> GenerateGlJournalBatchAsync(Guid payRunId, bool regenerate, CancellationToken cancellationToken = default);
    Task<List<GlJournalBatchSummaryDto>> GetGlJournalBatchesAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<GlJournalBatchDetailDto?> GetGlJournalBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<GlJournalBatchDetailDto> ApproveGlJournalBatchAsync(Guid batchId, GlJournalBatchActionRequest request, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> ExportGlJournalBatchAsync(Guid batchId, string format, CancellationToken cancellationToken = default);
    Task<List<GlAccountDto>> GetGlAccountsAsync(CancellationToken cancellationToken = default);
    Task<GlAccountDto> UpsertGlAccountAsync(UpsertGlAccountRequest request, CancellationToken cancellationToken = default);
    Task DeleteGlAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<List<GlMappingDto>> GetGlMappingsAsync(CancellationToken cancellationToken = default);
    Task<GlMappingDto> UpsertGlMappingAsync(UpsertGlMappingRequest request, CancellationToken cancellationToken = default);
    Task<ApitReportDto?> GetApitReportAsync(Guid payRunId, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> GenerateApitCertificateAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default);
    Task<FileExportResultDto?> ExportPaySlipAsync(Guid payRunId, Guid paySlipId, string format = "pdf", CancellationToken cancellationToken = default);
}
