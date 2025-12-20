using Payroll.Domain.Common;
using Payroll.Domain.Employees;
using Payroll.Domain.Organizations;

namespace Payroll.Domain.Payroll;

public class GeneratedTaxDocument : AuditableEntity
{
    public GeneratedTaxDocumentType Type { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public int? Year { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public GeneratedTaxDocumentStatus Status { get; set; } = GeneratedTaxDocumentStatus.Pending;
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? MetadataJson { get; set; }
    public string? ErrorSummary { get; set; }
}

public enum GeneratedTaxDocumentType
{
    MonthlyReport = 1,
    AnnualReport = 2,
    EmployeeCertificate = 3
}

public enum GeneratedTaxDocumentStatus
{
    Pending = 1,
    Generated = 2,
    Failed = 3
}
