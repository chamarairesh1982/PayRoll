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
        RuleFor(x => x.HourlyRate).GreaterThan(0).When(x => x.HourlyRate.HasValue);

        RuleFor(x => x.BankAccountNumber)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.BankName)
                || !string.IsNullOrWhiteSpace(x.BankCode)
                || !string.IsNullOrWhiteSpace(x.BranchCode))
            .WithMessage("Bank account number is required when a bank is selected.");

        RuleFor(x => x.BankName)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNumber))
            .WithMessage("Bank name is required when a bank account number is provided.");

        RuleFor(x => x.BankCode)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNumber))
            .WithMessage("Bank code is required when a bank account number is provided.");

        RuleFor(x => x.BranchCode)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNumber))
            .WithMessage("Branch code is required when a bank account number is provided.");
    }
}
