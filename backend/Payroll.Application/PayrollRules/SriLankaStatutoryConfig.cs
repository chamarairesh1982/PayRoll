namespace Payroll.Application.PayrollRules;

public class SriLankaStatutoryConfig
{
    public SriLankaEpEpfRules EpEpfRules { get; set; } = new();
    public SriLankaPayeeRules PayeeRules { get; set; } = new();
    public SriLankaOtRules OtRules { get; set; } = new();
}
