using FluentValidation;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.Validators.PayrollConfig;

public class CreateBankBranchRequestValidator : AbstractValidator<CreateBankBranchRequest>
{
    public CreateBankBranchRequestValidator()
    {
        RuleFor(x => x.BankId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(10)
            .Must(code => !code.Contains(' '))
            .WithMessage("Code must not contain spaces.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
