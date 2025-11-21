using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class EmployeeService : IEmployeeService
{
    public Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employee, CancellationToken cancellationToken = default)
    {
        // TODO: implement persistence logic
        return Task.FromResult(employee);
    }

    public Task DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: soft delete employee
        return Task.CompletedTask;
    }

    public Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: fetch from repository
        return Task.FromResult<EmployeeDto?>(null);
    }

    public Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // TODO: return paginated employees
        var result = new PaginatedResult<EmployeeDto>
        {
            Items = Array.Empty<EmployeeDto>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = 0
        };
        return Task.FromResult(result);
    }

    public Task UpdateEmployeeAsync(Guid id, EmployeeDto employee, CancellationToken cancellationToken = default)
    {
        // TODO: update employee data
        return Task.CompletedTask;
    }
}
