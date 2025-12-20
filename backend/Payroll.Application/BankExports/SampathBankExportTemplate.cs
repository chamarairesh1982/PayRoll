using System.Text;

namespace Payroll.Application.BankExports;

public class SampathBankExportTemplate : IBankExportTemplate
{
    public string TemplateName => "Sampath";
    public string FileExtension => "csv";
    public string ContentType => "text/csv";

    public string Render(IEnumerable<BankExportRow> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");

        foreach (var row in rows)
        {
            builder.Append(row.RowNo).Append(',');
            builder.Append('"').Append(row.BeneficiaryName.Replace("\"", "''")).Append('"').Append(',');
            builder.Append(row.AccountNumber).Append(',');
            builder.Append(row.Amount.ToString("F2")).Append(',');
            builder.Append(row.Reference).Append(',');
            builder.Append(row.EmployeeCode);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
