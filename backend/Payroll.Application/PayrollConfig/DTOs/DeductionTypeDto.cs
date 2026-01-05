using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.PayrollConfig.DTOs;

public class DeductionTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public CalculationBasis Basis { get; set; }

    public bool IsPreTax { get; set; }
    public bool IsPostTax { get; set; }

    public bool IsActive { get; set; }
}
