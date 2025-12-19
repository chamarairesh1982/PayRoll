using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class StatutoryReport : AuditableEntity
{
    public StatutoryReportType Type { get; set; }
    public Guid PayRunId { get; set; }
    public PayRun? PayRun { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public StatutoryReportStatus Status { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public int WarningCount { get; set; }
    public string? WarningFilePath { get; set; }
    public string? WarningFileName { get; set; }
    public string? WarningContentType { get; set; }
}

public enum StatutoryReportType
{
    EpfEtf = 1
}

public enum StatutoryReportStatus
{
    Generated = 1,
    Failed = 2
}
