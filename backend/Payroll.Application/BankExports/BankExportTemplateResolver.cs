namespace Payroll.Application.BankExports;

public class BankExportTemplateResolver
{
    private readonly Dictionary<string, IBankExportTemplate> _templates;

    public BankExportTemplateResolver()
    {
        _templates = new Dictionary<string, IBankExportTemplate>(StringComparer.OrdinalIgnoreCase)
        {
            { "HNB", new HnbBankExportTemplate() },
            { "BOC", new BocBankExportTemplate() },
            { "Commercial", new CommercialBankExportTemplate() },
            { "Sampath", new SampathBankExportTemplate() },
            { "DFCC", new DfccBankExportTemplate() }
        };
    }

    public IBankExportTemplate Resolve(string templateName)
    {
        if (_templates.TryGetValue(templateName, out var template))
        {
            return template;
        }

        throw new InvalidOperationException($"Unsupported bank export template: {templateName}");
    }
}
