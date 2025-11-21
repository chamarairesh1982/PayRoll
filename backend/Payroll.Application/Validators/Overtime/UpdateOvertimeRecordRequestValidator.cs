using FluentValidation;
using Payroll.Application.Overtime.DTOs;

namespace Payroll.Application.Validators.Overtime;

public class UpdateOvertimeRecordRequestValidator : AbstractValidator<UpdateOvertimeRecordRequest>
{
    public UpdateOvertimeRecordRequestValidator()
    {
        RuleFor(x => x.Hours!.Value)
            .GreaterThan(0)
            .When(x => x.Hours.HasValue);
    }
}
