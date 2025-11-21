namespace Payroll.Application.Payroll.DTOs;

public class PaySlipDto
{
    public Guid Id { get; set; }
    public Guid PayRunId { get; set; }
    public Guid EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }

    public decimal BasicSalary { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }

    public decimal EmployeeEpf { get; set; }
    public decimal EmployerEpf { get; set; }
    public decimal EmployerEtf { get; set; }
    public decimal PayeTax { get; set; }

    public string Currency { get; set; } = "LKR";

    public List<PaySlipEarningLineDto> Earnings { get; set; } = new();
    public List<PaySlipDeductionLineDto> Deductions { get; set; } = new();
}
