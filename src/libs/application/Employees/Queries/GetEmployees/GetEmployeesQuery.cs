using MediatR;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Common;
using Payroll.Contracts.Employees;

namespace Payroll.Application.Employees.Queries.GetEmployees;

public record GetEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, IEnumerable<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        // Global Query Filter automatically applies TenantId scoping
        return await _context.Employees
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                JoinDate = e.JoinDate,
                BaseSalary = e.BaseSalary
            })
            .ToListAsync(cancellationToken);
    }
}
