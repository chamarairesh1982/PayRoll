using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class PayRunSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PayPeriodType PeriodType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public PayRunStatus Status { get; set; }
    public bool IsLocked { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalNetPay { get; set; }
}

public class PayRunDetailDto : PayRunSummaryDto
{
    public List<PaySlipDto> PaySlips { get; set; } = new();
}
