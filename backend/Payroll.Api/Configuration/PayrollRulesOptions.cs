using Payroll.Application.PayrollRules;

namespace Payroll.Api.Configuration;

public class PayrollRulesOptions
{
    public SriLankaStatutoryConfig SriLanka { get; set; } = new();
}
