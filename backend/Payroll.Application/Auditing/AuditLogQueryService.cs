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

    public async Task<PaginatedResult<AuditEventDto>> GetAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = NormalizePaging(query.Page, query.PageSize);
        var logs = ApplyFilters(query);

        var totalCount = await logs.CountAsync(cancellationToken);
        var items = await logs
            .OrderByDescending(l => l.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
           .Select(l => new AuditEventDto
           {
               Id = l.Id,
               TimestampUtc = l.TimestampUtc,
               ActorUserId = l.ActorUserId,
               ActorDisplayName = l.ActorDisplayName,
               EntityType = l.EntityType,
               EntityId = l.EntityId,
               Action = l.Action,
               BeforeJson = l.BeforeJson,
               AfterJson = l.AfterJson,
               CorrelationId = l.CorrelationId
           })
            .ToListAsync(cancellationToken);

           

        return new PaginatedResult<AuditEventDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<AuditEventDto>> ExportAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var logs = ApplyFilters(query);
        return await logs
            .OrderByDescending(l => l.TimestampUtc)
            .Take(5000)
               .Select(l => new AuditEventDto
               {
                   Id = l.Id,
                   TimestampUtc = l.TimestampUtc,
                   ActorUserId = l.ActorUserId,
                   ActorDisplayName = l.ActorDisplayName,
                   EntityType = l.EntityType,
                   EntityId = l.EntityId,
                   Action = l.Action,
                   BeforeJson = l.BeforeJson,
                   AfterJson = l.AfterJson,
                   CorrelationId = l.CorrelationId
               })
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditEventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.AuditEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(log => log.Id == id, cancellationToken);

        return entity is null ? null : AuditEventDto.FromEntity(entity);
    }

    private IQueryable<Domain.Auditing.AuditEvent> ApplyFilters(AuditLogQuery query)
    {
        var logs = _dbContext.AuditEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            logs = logs.Where(l => l.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            logs = logs.Where(l => l.EntityId == query.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logs = logs.Where(l => l.Action == query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.Actor))
        {
            logs = logs.Where(l => l.ActorUserId == query.Actor || l.ActorDisplayName == query.Actor);
        }

        if (query.From.HasValue)
        {
            logs = logs.Where(l => l.TimestampUtc >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            var toInclusive = query.To.Value;
            logs = logs.Where(l => l.TimestampUtc <= toInclusive);
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
