namespace Payroll.Application.DTOs;

public record PayslipDocumentPayload
{
    public Guid PayRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeCode { get; init; }
    public string? EmployeeName { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public DateTime PayDate { get; init; }
    public decimal GrossPay { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetPay { get; init; }
    public decimal EmployeeEpf { get; init; }
    public decimal EmployerEpf { get; init; }
    public decimal EmployerEtf { get; init; }
    public decimal PayeTax { get; init; }
    public IReadOnlyList<PayslipDocumentLine> Earnings { get; init; } = Array.Empty<PayslipDocumentLine>();
    public IReadOnlyList<PayslipDocumentLine> Deductions { get; init; } = Array.Empty<PayslipDocumentLine>();
}

public record PayslipDocumentLine
{
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
