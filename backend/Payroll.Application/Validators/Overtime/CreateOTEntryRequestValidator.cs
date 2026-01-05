using FluentValidation;
using Payroll.Application.Overtime.DTOs;

namespace Payroll.Application.Validators.Overtime;

public class CreateOTEntryRequestValidator : AbstractValidator<CreateOTEntryRequest>
{
    public CreateOTEntryRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.WorkDate).NotEqual(default(DateTime));
        RuleFor(x => x.RawMinutes).GreaterThan(0);
        RuleFor(x => x.Status).IsInEnum();
    }
}
