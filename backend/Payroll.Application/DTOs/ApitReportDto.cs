using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class ApitReportDto
{
    public Guid PayRunId { get; set; }
    public string PayRunCode { get; set; } = string.Empty;
    public string PayRunName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public PayPeriodType PeriodType { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalTaxForPeriod { get; set; }
    public decimal TotalTaxYearToDate { get; set; }
    public List<ApitReportEmployeeDto> Employees { get; set; } = new();
}

public class ApitReportEmployeeDto
{
    public Guid PaySlipId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public decimal TaxableEarnings { get; set; }
    public decimal PreTaxDeductions { get; set; }
    public decimal ReliefAmount { get; set; }
    public decimal RebateAmount { get; set; }
    public decimal TaxableAfterRelief { get; set; }
    public decimal ApitWithheld { get; set; }
    public decimal YearToDateApit { get; set; }
}

public class FileExportResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/plain";
    public string ContentBase64 { get; set; } = string.Empty;
}
