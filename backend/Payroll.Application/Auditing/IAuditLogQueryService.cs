using Payroll.Shared;

namespace Payroll.Application.Auditing;

public interface IAuditLogQueryService
{
    Task<PaginatedResult<AuditLogDto>> GetAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogDto>> ExportAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
}
