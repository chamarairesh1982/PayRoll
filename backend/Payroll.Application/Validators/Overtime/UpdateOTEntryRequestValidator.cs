using FluentValidation;
using Payroll.Application.Overtime.DTOs;

namespace Payroll.Application.Validators.Overtime;

public class UpdateOTEntryRequestValidator : AbstractValidator<UpdateOTEntryRequest>
{
    public UpdateOTEntryRequestValidator()
    {
        RuleFor(x => x.Hours!.Value)
            .GreaterThan(0)
            .When(x => x.Hours.HasValue);
    }
}
