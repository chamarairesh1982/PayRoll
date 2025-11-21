using FluentValidation;
using Payroll.Application.Overtime.DTOs;

namespace Payroll.Application.Validators.Overtime;

public class CreateOvertimeRecordRequestValidator : AbstractValidator<CreateOvertimeRecordRequest>
{
    public CreateOvertimeRecordRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Date).NotEqual(default(DateTime));
        RuleFor(x => x.Hours).GreaterThan(0);
    }
}
