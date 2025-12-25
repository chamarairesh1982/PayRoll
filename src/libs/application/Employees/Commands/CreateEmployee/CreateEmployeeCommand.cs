using FluentValidation;
using MediatR;
using Payroll.Application.Common;
using Payroll.Domain.Employees;

namespace Payroll.Application.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand : IRequest<Guid>
{
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly JoinDate { get; init; }
    public decimal BaseSalary { get; init; }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(v => v.EmployeeCode)
            .NotEmpty().WithMessage("Employee Code is required.")
            .MaximumLength(50);

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First Name is required.")
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last Name is required.")
            .MaximumLength(100);

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
            
        RuleFor(v => v.BaseSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Base Salary must be positive.");
    }
}

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var entity = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            JoinDate = request.JoinDate,
            BaseSalary = request.BaseSalary
        };

        _context.Employees.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
