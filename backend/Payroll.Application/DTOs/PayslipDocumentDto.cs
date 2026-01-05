using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class PayslipDocumentDto
{
    public Guid Id { get; set; }
    public Guid PayRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public PayslipDocumentStatus Status { get; set; }
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public string? FileName { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? ErrorSummary { get; set; }
}
