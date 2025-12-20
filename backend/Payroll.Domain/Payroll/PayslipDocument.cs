using Payroll.Domain.Common;
using Payroll.Domain.Employees;

namespace Payroll.Domain.Payroll;

public class PayslipDocument : AuditableEntity
{
    public Guid PayRunId { get; set; }
    public PayRun? PayRun { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public PayslipDocumentStatus Status { get; set; } = PayslipDocumentStatus.Pending;
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? ErrorSummary { get; set; }
}
