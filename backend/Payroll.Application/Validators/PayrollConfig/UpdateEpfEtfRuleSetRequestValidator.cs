using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class UpdateEpfEtfRuleSetRequestValidator : AbstractValidator<UpdateEpfEtfRuleSetRequest>
{
    public UpdateEpfEtfRuleSetRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100);

        RuleFor(x => x.EmployeeEpfRate).GreaterThanOrEqualTo(0).When(x => x.EmployeeEpfRate.HasValue);
        RuleFor(x => x.EmployerEpfRate).GreaterThanOrEqualTo(0).When(x => x.EmployerEpfRate.HasValue);
        RuleFor(x => x.EmployerEtfRate).GreaterThanOrEqualTo(0).When(x => x.EmployerEtfRate.HasValue);

        RuleFor(x => x.MinimumWageForEpf).GreaterThanOrEqualTo(0).When(x => x.MinimumWageForEpf.HasValue);
        RuleFor(x => x.MaximumEarningForEpf).GreaterThanOrEqualTo(0).When(x => x.MaximumEarningForEpf.HasValue);
        RuleFor(x => x.MaximumEarningForEtf).GreaterThanOrEqualTo(0).When(x => x.MaximumEarningForEtf.HasValue);

        RuleFor(x => x)
            .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveTo.Value >= x.EffectiveFrom.Value)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");
    }
}
