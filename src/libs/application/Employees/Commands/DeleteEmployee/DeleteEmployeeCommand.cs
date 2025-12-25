using MediatR;
using Payroll.Application.Common;

namespace Payroll.Application.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.Id} not found.");
        }

        _context.Employees.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
