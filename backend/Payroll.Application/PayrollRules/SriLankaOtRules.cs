namespace Payroll.Application.PayrollRules;

public class SriLankaOtRules
{
    public decimal StandardRateMultiplier { get; set; } = 1.5m;
    public decimal WeekendRateMultiplier { get; set; } = 2m;
}
