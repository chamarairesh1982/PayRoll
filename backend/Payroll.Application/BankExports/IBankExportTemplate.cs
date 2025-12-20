namespace Payroll.Application.BankExports;

public interface IBankExportTemplate
{
    string TemplateName { get; }
    string FileExtension { get; }
    string ContentType { get; }
    string Render(IEnumerable<BankExportRow> rows);
}
