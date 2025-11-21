using FluentValidation;
using Payroll.Application.DTOs.Employees;

namespace Payroll.Application.Validators.Employees;

public class CreateEmployeeRequestDtoValidator : AbstractValidator<CreateEmployeeRequestDto>
{
    public CreateEmployeeRequestDtoValidator()
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
