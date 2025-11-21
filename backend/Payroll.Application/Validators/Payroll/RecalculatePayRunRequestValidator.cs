using FluentValidation;
using Payroll.Application.Payroll.Requests;

namespace Payroll.Application.Validators.Payroll;

public class RecalculatePayRunRequestValidator : AbstractValidator<RecalculatePayRunRequest>
{
    public RecalculatePayRunRequestValidator()
    {
        RuleFor(x => x)
            .NotNull();
    }
}
