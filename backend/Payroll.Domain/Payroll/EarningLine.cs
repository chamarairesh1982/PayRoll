namespace Payroll.Domain.Payroll;

public class EarningLine
{
    public Guid Id { get; set; }
    public Guid PaySlipId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsEpfApplicable { get; set; }
    public bool IsEtfApplicable { get; set; }
    public bool IsTaxable { get; set; }
}
