using Payroll.Shared;

namespace Payroll.Application.Auditing;

public interface IAuditLogQueryService
{
    Task<PaginatedResult<AuditEventDto>> GetAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEventDto>> ExportAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task<AuditEventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
