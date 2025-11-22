namespace Payroll.Domain.Payroll;

public class DeductionLine
{
    public Guid Id { get; set; }
    public Guid PaySlipId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPreTax { get; set; }
    public bool IsPostTax { get; set; }
}
