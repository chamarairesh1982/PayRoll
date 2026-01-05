namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateEpfEtfRuleSetRequest
{
    public string? Name { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public decimal? EmployeeEpfRate { get; set; }
    public decimal? EmployerEpfRate { get; set; }
    public decimal? EmployerEtfRate { get; set; }

    public decimal? MinimumWageForEpf { get; set; }
    public decimal? MaximumEarningForEpf { get; set; }
    public decimal? MaximumEarningForEtf { get; set; }

    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}
