using FluentValidation;
using Payroll.Application.Payroll.Requests;

namespace Payroll.Application.Validators.Payroll;

public class CreatePayRunRequestValidator : AbstractValidator<CreatePayRunRequest>
{
    public CreatePayRunRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.PeriodType)
            .IsInEnum();

        RuleFor(x => x.PeriodStart)
            .LessThanOrEqualTo(x => x.PeriodEnd)
            .WithMessage("Period start must be before or equal to period end.");

        RuleFor(x => x.PayDate)
            .GreaterThanOrEqualTo(x => x.PeriodStart)
            .WithMessage("Pay date should be on or after the period start.");
    }
}
