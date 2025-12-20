using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class UpdateBankRequestValidator : AbstractValidator<UpdateBankRequest>
{
    public UpdateBankRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(10)
            .Must(code => !code.Contains(' '))
            .WithMessage("Code must not contain spaces.")
            .When(x => x.Code != null);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Name != null);
    }
}
