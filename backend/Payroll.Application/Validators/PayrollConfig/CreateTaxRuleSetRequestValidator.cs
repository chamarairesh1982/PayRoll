using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class CreateTaxRuleSetRequestValidator : AbstractValidator<CreateTaxRuleSetRequest>
{
    public CreateTaxRuleSetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.YearOfAssessment)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo.Value >= x.EffectiveFrom)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");

        RuleFor(x => x.Slabs)
            .NotNull()
            .Must(slabs => slabs.Count > 0)
            .WithMessage("At least one tax slab is required.");

        RuleForEach(x => x.Slabs).SetValidator(new CreateTaxSlabItemValidator());
    }
}

public class CreateTaxSlabItemValidator : AbstractValidator<CreateTaxSlabItem>
{
    public CreateTaxSlabItemValidator()
    {
        RuleFor(x => x.FromAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RatePercent).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ToAmount)
            .GreaterThan(x => x.FromAmount)
            .When(x => x.ToAmount.HasValue);
    }
}
