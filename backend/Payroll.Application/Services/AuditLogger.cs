using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.Utilities;
using Payroll.Domain.Auditing;

namespace Payroll.Application.Services;

public class AuditLogger : IAuditLogger
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogger(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(
        string entityName,
        string entityId,
        string action,
        object? beforeSnapshot,
        object? afterSnapshot,
        string? performedBy = null,
        CancellationToken cancellationToken = default)
    {
        var actorDisplayName = string.IsNullOrWhiteSpace(performedBy)
            ? _currentUserService.UserName ?? "system"
            : performedBy;

        var actorUserId = _currentUserService.UserId ?? "system";
        var timestampUtc = DateTime.UtcNow;
        var beforeJson = Serialize(beforeSnapshot);
        var afterJson = Serialize(afterSnapshot);

        var previousHash = await _dbContext.AuditEvents
            .AsNoTracking()
            .OrderByDescending(e => e.TimestampUtc)
            .ThenByDescending(e => e.Id)
            .Select(e => e.Hash)
            .FirstOrDefaultAsync(cancellationToken);

        var previousHashValue = previousHash ?? string.Empty;
        var hashPayload = string.Join('|',
            entityName,
            entityId,
            action,
            actorUserId,
            actorDisplayName,
            timestampUtc.ToString("O"),
            previousHashValue,
            beforeJson ?? string.Empty,
            afterJson ?? string.Empty,
            string.Empty);

        var logEntry = new AuditEvent
        {
            Id = Guid.NewGuid(),
            TimestampUtc = timestampUtc,
            ActorUserId = actorUserId,
            ActorDisplayName = actorDisplayName,
            EntityType = entityName,
            EntityId = entityId,
            Action = action,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            CorrelationId = null,
            PreviousHash = previousHashValue,
            Hash = HashingHelper.ComputeSha256Hash(hashPayload)
        };

        await _dbContext.AuditEvents.AddAsync(logEntry, cancellationToken);
    }

    private static string? Serialize(object? snapshot)
    {
        return snapshot is null
            ? null
            : JsonSerializer.Serialize(snapshot, SerializerOptions);
    }
}
