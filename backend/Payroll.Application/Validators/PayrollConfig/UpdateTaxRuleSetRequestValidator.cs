using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class UpdateTaxRuleSetRequestValidator : AbstractValidator<UpdateTaxRuleSetRequest>
{
    public UpdateTaxRuleSetRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100);

        RuleFor(x => x.YearOfAssessment)
            .GreaterThan(0)
            .When(x => x.YearOfAssessment.HasValue);

        RuleFor(x => x)
            .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveTo.Value >= x.EffectiveFrom.Value)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");

        When(x => x.Slabs != null, () =>
        {
            RuleForEach(x => x.Slabs!).SetValidator(new UpdateTaxSlabItemValidator());
        });
    }
}

public class UpdateTaxSlabItemValidator : AbstractValidator<UpdateTaxSlabItem>
{
    public UpdateTaxSlabItemValidator()
    {
        RuleFor(x => x.FromAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RatePercent).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ToAmount)
            .GreaterThan(x => x.FromAmount)
            .When(x => x.ToAmount.HasValue);
    }
}
