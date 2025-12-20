using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class MonthlyTaxReportRequestDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string Format { get; set; } = "csv";
}

public class AnnualTaxReportRequestDto
{
    public int Year { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string Format { get; set; } = "csv";
}

public class TaxCertificateRequestDto
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
}

public class TaxDocumentMetadataDto
{
    public Guid Id { get; set; }
    public GeneratedTaxDocumentType Type { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public int? Year { get; set; }
    public Guid? EmployeeId { get; set; }
    public GeneratedTaxDocumentStatus Status { get; set; }
    public DateTime? GeneratedAtUtc { get; set; }
    public string? FileName { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ErrorSummary { get; set; }
}

public class TaxDocumentHistoryDto
{
    public Guid Id { get; set; }
    public GeneratedTaxDocumentType Type { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public int? Year { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public GeneratedTaxDocumentStatus Status { get; set; }
    public DateTime? GeneratedAtUtc { get; set; }
    public string? FileName { get; set; }
}
