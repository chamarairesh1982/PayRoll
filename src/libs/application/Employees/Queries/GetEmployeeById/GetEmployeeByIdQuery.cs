using MediatR;
using Payroll.Application.Common;
using Payroll.Contracts.Employees;

namespace Payroll.Application.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null) return null;

        return new EmployeeDto
        {
            Id = entity.Id,
            EmployeeCode = entity.EmployeeCode,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            JoinDate = entity.JoinDate,
            BaseSalary = entity.BaseSalary
        };
    }
}
