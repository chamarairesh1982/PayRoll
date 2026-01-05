using FluentAssertions;
using Payroll.Application.BankExports;
using Xunit;

namespace Payroll.Application.Tests.BankExports;

public class BankExportTemplateTests
{
    [Fact]
    public void HnbTemplate_Should_Render_Header_And_Row()
    {
        var template = new HnbBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow(1, "Alice", "12345678", 1500.50m, "PR001", "EMP1")
        });

        content.Should().Contain("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");
        content.Should().Contain("1,\"Alice\",12345678,1500.50,PR001,EMP1");
    }

    [Fact]
    public void BocTemplate_Should_Render_Header_And_Row()
    {
        var template = new BocBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow(1, "Bob", "78901234", 999.99m, "PR002", "EMP2")
        });

        content.Should().Contain("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");
        content.Should().Contain("1,\"Bob\",78901234,999.99,PR002,EMP2");
    }

    [Fact]
    public void CommercialTemplate_Should_Render_Header_And_Row()
    {
        var template = new CommercialBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow(1, "Chloe", "55555123", 100m, "PR003", "EMP3")
        });

        content.Should().Contain("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");
        content.Should().Contain("1,\"Chloe\",55555123,100.00,PR003,EMP3");
    }

    [Fact]
    public void SampathTemplate_Should_Render_Header_And_Row()
    {
        var template = new SampathBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow(1, "Dana", "12344321", 250.75m, "PR004", "EMP4")
        });

        content.Should().Contain("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");
        content.Should().Contain("1,\"Dana\",12344321,250.75,PR004,EMP4");
    }

    [Fact]
    public void DfccTemplate_Should_Render_Header_And_Row()
    {
        var template = new DfccBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow(1, "Evan", "77778888", 333.33m, "PR005", "EMP5")
        });

        content.Should().Contain("RowNo,BeneficiaryName,AccountNumber,Amount,Reference,EmployeeCode");
        content.Should().Contain("1,\"Evan\",77778888,333.33,PR005,EMP5");
    }
}
