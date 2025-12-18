using System.Text;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Auditing;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogQueryService _auditLogQueryService;

    public AuditLogsController(IAuditLogQueryService auditLogQueryService)
    {
        _auditLogQueryService = auditLogQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Export)
        {
            var exportRows = await _auditLogQueryService.ExportAsync(query, cancellationToken);
            var csv = BuildCsv(exportRows);
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        var result = await _auditLogQueryService.GetAsync(query, cancellationToken);
        return Ok(result);
    }

    private static string BuildCsv(IEnumerable<AuditLogDto> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Timestamp,Actor,Entity,EntityId,Action,Before,After");

        foreach (var row in rows)
        {
            builder.Append(Escape(row.CreatedAt.ToString("O"))).Append(',')
                .Append(Escape(row.CreatedBy)).Append(',')
                .Append(Escape(row.EntityName)).Append(',')
                .Append(Escape(row.EntityId)).Append(',')
                .Append(Escape(row.Action)).Append(',')
                .Append(Escape(row.BeforeSnapshot)).Append(',')
                .AppendLine(Escape(row.AfterSnapshot));
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
}
