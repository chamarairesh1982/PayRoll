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
            new BankExportRow("EMP1", "Alice", "123456", "001", 1500.50m)
        }, "PR001");

        content.Should().Contain("AccountNumber,Amount,Name,Reference");
        content.Should().Contain("123456,1500.50,\"Alice\",PR001");
    }

    [Fact]
    public void BocTemplate_Should_Pipe_Separate_Columns()
    {
        var template = new BocBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow("EMP2", "Bob", "7890", "BR01", 999.99m)
        }, "PR002");

        content.Trim().Should().Be("BR01|7890|Bob|999.99|PR002");
    }

    [Fact]
    public void CommercialTemplate_Should_Combine_Code_And_Name()
    {
        var template = new CommercialBankExportTemplate();
        var content = template.Render(new[]
        {
            new BankExportRow("EMP3", "Chloe", "55555", "CMB1", 100m)
        }, "PR003");

        content.Should().Contain("CMB1,55555,100.00,EMP3-Chloe,PR003");
    }
}
