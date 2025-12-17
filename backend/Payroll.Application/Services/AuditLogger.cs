using System.Text.Json;
using System.Text.Json.Serialization;
using Payroll.Application.Interfaces;
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
        var createdBy = string.IsNullOrWhiteSpace(performedBy)
            ? _currentUserService.UserName ?? "system"
            : performedBy;

        var logEntry = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            BeforeSnapshot = Serialize(beforeSnapshot),
            AfterSnapshot = Serialize(afterSnapshot),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _dbContext.AuditLogs.AddAsync(logEntry, cancellationToken);
    }

    private static string Serialize(object? snapshot)
    {
        return snapshot is null
            ? string.Empty
            : JsonSerializer.Serialize(snapshot, SerializerOptions);
    }
}
