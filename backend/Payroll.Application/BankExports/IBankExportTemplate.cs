namespace Payroll.Application.BankExports;

public interface IBankExportTemplate
{
    string Bank { get; }
    string FileExtension { get; }
    string ContentType { get; }
    string Render(IEnumerable<BankExportRow> rows, string reference);
}
