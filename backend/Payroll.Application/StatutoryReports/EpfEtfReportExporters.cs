using System.Globalization;
using System.Text;

namespace Payroll.Application.StatutoryReports;

public interface IEpfEtfReportExporter
{
    string Format { get; }
    string FileExtension { get; }
    string ContentType { get; }
    string Render(EpfEtfReportData report);
}

public class EpfEtfReportExporterResolver
{
    private readonly Dictionary<string, IEpfEtfReportExporter> _exporters;

    public EpfEtfReportExporterResolver()
    {
        _exporters = new Dictionary<string, IEpfEtfReportExporter>(StringComparer.OrdinalIgnoreCase)
        {
            { "csv", new EpfEtfCsvExporter() },
            { "txt", new EpfEtfTxtExporter() }
        };
    }

    public IEpfEtfReportExporter Resolve(string format)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
        {
            throw new ArgumentException($"Unsupported EPF/ETF export format '{format}'.");
        }

        return exporter;
    }
}

public class EpfEtfCsvExporter : IEpfEtfReportExporter
{
    public string Format => "csv";
    public string FileExtension => "csv";
    public string ContentType => "text/csv";

    public string Render(EpfEtfReportData report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("EmployeeCode,EmployeeName,NIC,EPFNumber,ContributableBase,EmployeeEPF,EmployerEPF,EmployerETF");

        foreach (var row in report.Rows)
        {
            builder.AppendLine(string.Join(",",
                Escape(row.EmployeeCode),
                Escape(row.EmployeeName),
                Escape(row.NicNumber),
                Escape(row.EpfNumber),
                FormatAmount(row.ContributableBase),
                FormatAmount(row.EmployeeEpf),
                FormatAmount(row.EmployerEpf),
                FormatAmount(row.EmployerEtf)));
        }

        builder.AppendLine(string.Join(",",
            "TOTAL",
            string.Empty,
            string.Empty,
            string.Empty,
            FormatAmount(report.Summary.ContributableBaseTotal),
            FormatAmount(report.Summary.EmployeeEpfTotal),
            FormatAmount(report.Summary.EmployerEpfTotal),
            FormatAmount(report.Summary.EmployerEtfTotal)));

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sanitized = value.Replace("\"", "\"\"");
        return $"\"{sanitized}\"";
    }

    private static string FormatAmount(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}

public class EpfEtfTxtExporter : IEpfEtfReportExporter
{
    public string Format => "txt";
    public string FileExtension => "txt";
    public string ContentType => "text/plain";

    public string Render(EpfEtfReportData report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("EPF/ETF REPORT (SAMPLE FORMAT)");
        builder.AppendLine($"Pay Run: {report.PayRunCode} - {report.PayRunName}");
        builder.AppendLine($"Period: {report.PeriodStart:yyyy-MM-dd} to {report.PeriodEnd:yyyy-MM-dd}");
        builder.AppendLine($"Pay Date: {report.PayDate:yyyy-MM-dd}");
        builder.AppendLine("EmployeeCode|EmployeeName|NIC|EPFNumber|ContributableBase|EmployeeEPF|EmployerEPF|EmployerETF");

        foreach (var row in report.Rows)
        {
            builder.AppendLine(string.Join("|",
                row.EmployeeCode ?? string.Empty,
                row.EmployeeName ?? string.Empty,
                row.NicNumber ?? string.Empty,
                row.EpfNumber ?? string.Empty,
                FormatAmount(row.ContributableBase),
                FormatAmount(row.EmployeeEpf),
                FormatAmount(row.EmployerEpf),
                FormatAmount(row.EmployerEtf)));
        }

        builder.AppendLine(string.Join("|",
            "TOTALS",
            string.Empty,
            string.Empty,
            string.Empty,
            FormatAmount(report.Summary.ContributableBaseTotal),
            FormatAmount(report.Summary.EmployeeEpfTotal),
            FormatAmount(report.Summary.EmployerEpfTotal),
            FormatAmount(report.Summary.EmployerEtfTotal)));
        return builder.ToString();
    }

    private static string FormatAmount(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
