using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.PayrollConfig.DTOs;

public class CreateDeductionTypeRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CalculationBasis Basis { get; set; }
    public bool IsPreTax { get; set; }
    public bool IsPostTax { get; set; }
}
