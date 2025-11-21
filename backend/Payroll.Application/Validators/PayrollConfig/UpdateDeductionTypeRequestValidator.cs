using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class UpdateDeductionTypeRequestValidator : AbstractValidator<UpdateDeductionTypeRequest>
{
    public UpdateDeductionTypeRequestValidator()
    {
        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        });

        RuleFor(x => x.Basis)
            .IsInEnum()
            .When(x => x.Basis.HasValue);
    }
}
