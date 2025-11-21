using Payroll.Application.DTOs;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employee, CancellationToken cancellationToken = default);
    Task UpdateEmployeeAsync(Guid id, EmployeeDto employee, CancellationToken cancellationToken = default);
    Task DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);
}
