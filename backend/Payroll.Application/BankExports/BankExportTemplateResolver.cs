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
            { "Commercial", new CommercialBankExportTemplate() }
        };
    }

    public IBankExportTemplate Resolve(string bank)
    {
        if (string.IsNullOrWhiteSpace(bank))
        {
            throw new ArgumentException("Bank is required", nameof(bank));
        }

        if (_templates.TryGetValue(bank.Trim(), out var template))
        {
            return template;
        }

        throw new KeyNotFoundException($"Unsupported bank export template '{bank}'.");
    }
}
