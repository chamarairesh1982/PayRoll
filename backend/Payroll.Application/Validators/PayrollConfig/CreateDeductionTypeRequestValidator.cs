using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class CreateDeductionTypeRequestValidator : AbstractValidator<CreateDeductionTypeRequest>
{
    public CreateDeductionTypeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Must(code => !code.Contains(' '))
            .WithMessage("Code must not contain spaces.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Basis)
            .IsInEnum();
    }
}
