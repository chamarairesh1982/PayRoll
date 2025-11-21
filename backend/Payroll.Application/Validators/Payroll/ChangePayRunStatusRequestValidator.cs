using FluentValidation;
using Payroll.Application.Payroll.Requests;

namespace Payroll.Application.Validators.Payroll;

public class ChangePayRunStatusRequestValidator : AbstractValidator<ChangePayRunStatusRequest>
{
    public ChangePayRunStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
