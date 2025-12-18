using System.Text;

namespace Payroll.Application.BankExports;

public class HnbBankExportTemplate : IBankExportTemplate
{
    public string Bank => "HNB";
    public string FileExtension => "csv";
    public string ContentType => "text/csv";

    public string Render(IEnumerable<BankExportRow> rows, string reference)
    {
        var builder = new StringBuilder();
        builder.AppendLine("AccountNumber,Amount,Name,Reference");

        foreach (var row in rows)
        {
            builder.Append(row.AccountNumber).Append(',');
            builder.Append(row.Amount.ToString("F2")).Append(',');
            builder.Append('"').Append(row.EmployeeName.Replace("\"", "''")).Append('"').Append(',');
            builder.Append(reference);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
