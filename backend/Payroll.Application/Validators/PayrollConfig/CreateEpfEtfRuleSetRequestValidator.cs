using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class CreateEpfEtfRuleSetRequestValidator : AbstractValidator<CreateEpfEtfRuleSetRequest>
{
    public CreateEpfEtfRuleSetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.EmployeeEpfRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EmployerEpfRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EmployerEtfRate).GreaterThanOrEqualTo(0);

        RuleFor(x => x.MinimumWageForEpf).GreaterThanOrEqualTo(0).When(x => x.MinimumWageForEpf.HasValue);
        RuleFor(x => x.MaximumEarningForEpf).GreaterThanOrEqualTo(0).When(x => x.MaximumEarningForEpf.HasValue);
        RuleFor(x => x.MaximumEarningForEtf).GreaterThanOrEqualTo(0).When(x => x.MaximumEarningForEtf.HasValue);

        RuleFor(x => x)
            .Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo.Value >= x.EffectiveFrom)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");
    }
}
