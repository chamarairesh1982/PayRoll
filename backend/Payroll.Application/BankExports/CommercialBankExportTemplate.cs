using System.Text;

namespace Payroll.Application.BankExports;

public class CommercialBankExportTemplate : IBankExportTemplate
{
    public string Bank => "Commercial";
    public string FileExtension => "csv";
    public string ContentType => "text/csv";

    public string Render(IEnumerable<BankExportRow> rows, string reference)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Branch,Account,Amount,Employee,Reference");

        foreach (var row in rows)
        {
            builder
                .Append(row.BranchCode).Append(',')
                .Append(row.AccountNumber).Append(',')
                .Append(row.Amount.ToString("F2")).Append(',')
                .Append(row.EmployeeCode).Append('-').Append(row.EmployeeName).Append(',')
                .Append(reference)
                .AppendLine();
        }

        return builder.ToString();
    }
}
