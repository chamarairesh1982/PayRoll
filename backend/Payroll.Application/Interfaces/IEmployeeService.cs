using Payroll.Application.DTOs.Employees;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateEmployeeRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
