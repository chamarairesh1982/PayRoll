using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Shared;

namespace Payroll.Application.Auditing;

public class AuditLogQueryService : IAuditLogQueryService
{
    private readonly IPayrollDbContext _dbContext;

    public AuditLogQueryService(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<AuditLogDto>> GetAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = NormalizePaging(query.Page, query.PageSize);
        var logs = ApplyFilters(query);

        var totalCount = await logs.CountAsync(cancellationToken);
        var items = await logs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(AuditLogDto.FromEntity)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AuditLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<AuditLogDto>> ExportAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var logs = ApplyFilters(query);
        return await logs
            .OrderByDescending(l => l.CreatedAt)
            .Take(5000)
            .Select(AuditLogDto.FromEntity)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Domain.Auditing.AuditLog> ApplyFilters(AuditLogQuery query)
    {
        var logs = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            logs = logs.Where(l => l.EntityName == query.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            logs = logs.Where(l => l.EntityId == query.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logs = logs.Where(l => l.Action == query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.CreatedBy))
        {
            logs = logs.Where(l => l.CreatedBy == query.CreatedBy);
        }

        if (query.From.HasValue)
        {
            logs = logs.Where(l => l.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            var toInclusive = query.To.Value;
            logs = logs.Where(l => l.CreatedAt <= toInclusive);
        }

        return logs;
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 200);
        return (normalizedPage, normalizedPageSize);
    }
}
