using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class BankExportRequest
{
    public string Bank { get; set; } = string.Empty;
}

public class BankExportFailureDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class BankExportResultDto
{
    public BankExportStatus Status { get; set; }
    public string Bank { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/plain";
    public string ContentBase64 { get; set; } = string.Empty;
    public List<BankExportFailureDto> Failures { get; set; } = new();
}
