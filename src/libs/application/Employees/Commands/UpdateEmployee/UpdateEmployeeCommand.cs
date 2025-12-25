using FluentValidation;
using MediatR;
using Payroll.Application.Common;
using Payroll.Application.Common.Exceptions;

namespace Payroll.Application.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand : IRequest
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public decimal BaseSalary { get; init; }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();

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

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            // Note: In real app, we might use a custom NotFoundException
            throw new KeyNotFoundException($"Employee with ID {request.Id} not found.");
        }

        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Email = request.Email;
        entity.BaseSalary = request.BaseSalary;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
