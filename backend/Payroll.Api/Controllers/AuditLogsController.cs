using System.Text;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Auditing;
using Payroll.Application.Interfaces;
using Payroll.Application.Exceptions;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogQueryService _auditLogQueryService;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogsController(IAuditLogQueryService auditLogQueryService, ICurrentUserService currentUserService)
    {
        _auditLogQueryService = auditLogQueryService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        EnsureAuditAccess();

        if (query.Export)
        {
            var exportRows = await _auditLogQueryService.ExportAsync(query, cancellationToken);
            var csv = BuildCsv(exportRows);
            var fileName = $"audit-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        var result = await _auditLogQueryService.GetAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAuditAccess();

        var result = await _auditLogQueryService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private static string BuildCsv(IEnumerable<AuditEventDto> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Timestamp,ActorUserId,ActorDisplayName,EntityType,EntityId,Action,Before,After,CorrelationId");

        foreach (var row in rows)
        {
            builder.Append(Escape(row.TimestampUtc.ToString("O"))).Append(',')
                .Append(Escape(row.ActorUserId)).Append(',')
                .Append(Escape(row.ActorDisplayName ?? string.Empty)).Append(',')
                .Append(Escape(row.EntityType)).Append(',')
                .Append(Escape(row.EntityId)).Append(',')
                .Append(Escape(row.Action)).Append(',')
                .Append(Escape(row.BeforeJson ?? string.Empty)).Append(',')
                .Append(Escape(row.AfterJson ?? string.Empty)).Append(',')
                .AppendLine(Escape(row.CorrelationId ?? string.Empty));
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n');
        var sanitized = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{sanitized}\"" : sanitized;
    }

    private void EnsureAuditAccess()
    {
        var hasRole = _currentUserService.Roles.Any(role => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase));
        if (!hasRole)
        {
            throw new ForbiddenAccessException("Only admin users can access audit logs.");
        }
    }
}
