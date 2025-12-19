using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class EpfEtfReportRequestDto
{
    public Guid PayRunId { get; set; }
    public string Format { get; set; } = "csv";
}

public class EpfEtfReportResultDto
{
    public Guid ReportId { get; set; }
    public Guid PayRunId { get; set; }
    public string PayRunCode { get; set; } = string.Empty;
    public string PayRunName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public int EmployeeCount { get; set; }
    public decimal ContributableBase { get; set; }
    public decimal EmployeeEpfTotal { get; set; }
    public decimal EmployerEpfTotal { get; set; }
    public decimal EmployerEtfTotal { get; set; }
    public FileExportResultDto File { get; set; } = new();
    public FileExportResultDto? WarningFile { get; set; }
    public List<EpfEtfReportEmployeeDto> Employees { get; set; } = new();
    public List<EpfEtfReportWarningDto> Warnings { get; set; } = new();
}

public class EpfEtfReportEmployeeDto
{
    public Guid PaySlipId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? NicNumber { get; set; }
    public string? EpfNumber { get; set; }
    public decimal ContributableBase { get; set; }
    public decimal EmployeeEpf { get; set; }
    public decimal EmployerEpf { get; set; }
    public decimal EmployerEtf { get; set; }
}

public class EpfEtfReportWarningDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class StatutoryReportHistoryDto
{
    public Guid Id { get; set; }
    public StatutoryReportType Type { get; set; }
    public Guid PayRunId { get; set; }
    public string PayRunCode { get; set; } = string.Empty;
    public string PayRunName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public StatutoryReportStatus Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int WarningCount { get; set; }
}
