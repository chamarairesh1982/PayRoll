namespace Payroll.Application.PayrollRules;

public class SriLankaPayeeRules
{
    public decimal PersonalRelief { get; set; } = 1200000m;
    public decimal TaxFreeThreshold { get; set; } = 100000m;
}
