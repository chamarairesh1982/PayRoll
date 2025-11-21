using FluentValidation;
using Payroll.Application.DTOs.Employees;

namespace Payroll.Application.Validators.Employees;

public class UpdateEmployeeRequestDtoValidator : AbstractValidator<UpdateEmployeeRequestDto>
{
    public UpdateEmployeeRequestDtoValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.NicNumber).NotEmpty();
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.Today);
        RuleFor(x => x.EmploymentStartDate).NotEmpty();
        RuleFor(x => x.BaseSalary).GreaterThan(0);
    }
}
