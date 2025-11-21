namespace Payroll.Application.PayrollConfig.DTOs;

public class TaxSlabDto
{
    public Guid Id { get; set; }
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal RatePercent { get; set; }
    public int Order { get; set; }
}
