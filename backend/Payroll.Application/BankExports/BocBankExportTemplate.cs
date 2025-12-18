using System.Text;

namespace Payroll.Application.BankExports;

public class BocBankExportTemplate : IBankExportTemplate
{
    public string Bank => "BOC";
    public string FileExtension => "txt";
    public string ContentType => "text/plain";

    public string Render(IEnumerable<BankExportRow> rows, string reference)
    {
        var builder = new StringBuilder();
        foreach (var row in rows)
        {
            var line = string.Join('|', new[]
            {
                row.BranchCode,
                row.AccountNumber,
                row.EmployeeName,
                row.Amount.ToString("F2"),
                reference
            });
            builder.AppendLine(line);
        }

        return builder.ToString();
    }
}
