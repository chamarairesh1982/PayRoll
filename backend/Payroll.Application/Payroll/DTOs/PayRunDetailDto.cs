namespace Payroll.Application.Payroll.DTOs;

public class PayRunDetailDto : PayRunSummaryDto
{
    public List<PaySlipDto> PaySlips { get; set; } = new();
}
