namespace Payroll.Application.Payroll.DTOs;

public class PaySlipDeductionLineDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool IsPreTax { get; set; }
    public bool IsPostTax { get; set; }
}
