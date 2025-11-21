namespace Payroll.Application.Payroll.DTOs;

public class PaySlipEarningLineDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool IsEpfApplicable { get; set; }
    public bool IsEtfApplicable { get; set; }
    public bool IsTaxable { get; set; }
}
