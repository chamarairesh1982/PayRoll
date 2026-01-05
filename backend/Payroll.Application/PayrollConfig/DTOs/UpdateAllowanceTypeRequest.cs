using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateAllowanceTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public CalculationBasis? Basis { get; set; }
    public bool? IsEpfApplicable { get; set; }
    public bool? IsEtfApplicable { get; set; }
    public bool? IsTaxable { get; set; }
    public bool? IsActive { get; set; }
}
