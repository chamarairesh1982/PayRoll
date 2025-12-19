namespace Payroll.Application.Auditing;

public class AuditLogQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public string? Actor { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool Export { get; set; }
}
