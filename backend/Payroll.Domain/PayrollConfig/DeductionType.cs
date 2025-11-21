using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class DeductionType : AuditableEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public CalculationBasis Basis { get; set; }

    public bool IsPreTax { get; set; }
    public bool IsPostTax { get; set; }
}
