using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class AllowanceType : AuditableEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public CalculationBasis Basis { get; set; }

    public bool IsEpfApplicable { get; set; }
    public bool IsEtfApplicable { get; set; }
    public bool IsTaxable { get; set; }
}
