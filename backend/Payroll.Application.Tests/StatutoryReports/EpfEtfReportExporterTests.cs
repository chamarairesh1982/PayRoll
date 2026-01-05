using FluentAssertions;
using Payroll.Application.StatutoryReports;
using Xunit;

namespace Payroll.Application.Tests.StatutoryReports;

public class EpfEtfReportExporterTests
{
    [Fact]
    public void CsvExporter_Should_Render_Rows_And_Totals()
    {
        var report = BuildSampleReport();
        var exporter = new EpfEtfCsvExporter();

        var content = exporter.Render(report);

        content.Should().Contain("EmployeeCode,EmployeeName,NIC,EPFNumber,ContributableBase,EmployeeEPF,EmployerEPF,EmployerETF");
        content.Should().Contain("\"EMP01\",\"Ada Lovelace\",\"NIC1\",\"EPF1\",100000.00,8000.00,12000.00,3000.00");
        content.Should().Contain("TOTAL");
        content.Should().Contain("100000.00,8000.00,12000.00,3000.00");
    }

    [Fact]
    public void TxtExporter_Should_Render_Sample_Format()
    {
        var report = BuildSampleReport();
        var exporter = new EpfEtfTxtExporter();

        var content = exporter.Render(report);

        content.Should().Contain("EPF/ETF REPORT (SAMPLE FORMAT)");
        content.Should().Contain("EmployeeCode|EmployeeName|NIC|EPFNumber|ContributableBase|EmployeeEPF|EmployerEPF|EmployerETF");
        content.Should().Contain("EMP01|Ada Lovelace|NIC1|EPF1|100000.00|8000.00|12000.00|3000.00");
        content.Should().Contain("TOTALS");
    }

    private static EpfEtfReportData BuildSampleReport()
    {
        var summary = new EpfEtfReportSummary(1, 100000m, 8000m, 12000m, 3000m);
        var rows = new[]
        {
            new EpfEtfReportRow("EMP01", "Ada Lovelace", "NIC1", "EPF1", 100000m, 8000m, 12000m, 3000m)
        };
        return new EpfEtfReportData(
            "PR001",
            "April Payroll",
            new DateTime(2025, 4, 1),
            new DateTime(2025, 4, 30),
            new DateTime(2025, 4, 30),
            summary,
            rows);
    }
}
