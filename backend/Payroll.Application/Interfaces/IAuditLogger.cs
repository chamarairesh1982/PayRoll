namespace Payroll.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(
        string entityName,
        string entityId,
        string action,
        object? beforeSnapshot,
        object? afterSnapshot,
        string? performedBy = null,
        CancellationToken cancellationToken = default);
}
