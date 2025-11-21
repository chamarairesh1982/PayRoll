using FluentValidation;
using Payroll.Application.Leave.DTOs;

namespace Payroll.Application.Validators.Leave;

public class UpdateLeaveRequestRequestValidator : AbstractValidator<UpdateLeaveRequestRequest>
{
    public UpdateLeaveRequestRequestValidator()
    {
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate.Value)
                .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                .WithMessage("End date must be on or after start date.");
        });

        RuleFor(x => x.StartDate)
            .Must(d => d is null || d != default)
            .WithMessage("Start date is required when provided.");

        RuleFor(x => x.EndDate)
            .Must(d => d is null || d != default)
            .WithMessage("End date is required when provided.");

        When(x => x.IsHalfDay == true, () =>
        {
            RuleFor(x => x.HalfDaySession)
                .NotEmpty()
                .MaximumLength(2);
        });
    }
}
