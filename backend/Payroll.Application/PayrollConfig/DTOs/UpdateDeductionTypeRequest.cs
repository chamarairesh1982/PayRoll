using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateDeductionTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public CalculationBasis? Basis { get; set; }
    public bool? IsPreTax { get; set; }
    public bool? IsPostTax { get; set; }
    public bool? IsActive { get; set; }
}
