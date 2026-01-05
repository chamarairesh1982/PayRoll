using FluentValidation;
using Payroll.Application.Leave.DTOs;

namespace Payroll.Application.Validators.Leave;

public class CreateLeaveRequestRequestValidator : AbstractValidator<CreateLeaveRequestRequest>
{
    public CreateLeaveRequestRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");

        When(x => x.IsHalfDay == true, () =>
        {
            RuleFor(x => x.HalfDaySession)
                .NotEmpty()
                .MaximumLength(2);
        });
    }
}
