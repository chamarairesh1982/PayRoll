using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs.Employees;
using Payroll.Application.Interfaces;
using Payroll.Domain.Employees;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.EmployeeCode);

        var totalCount = await query.CountAsync(cancellationToken);
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = employees.Select(MapToDto).ToList();

        return new PaginatedResult<EmployeeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return employee is null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeCodeIsUniqueAsync(request.EmployeeCode, cancellationToken);
        await EnsureNicNumberIsUniqueAsync(request.NicNumber, cancellationToken);

        var createdBy = _currentUserService.UserName ?? "system";

        var employee = Employee.Create(
            request.EmployeeCode,
            request.FirstName,
            request.LastName,
            request.NicNumber,
            request.DateOfBirth,
            request.Gender,
            request.MaritalStatus,
            request.EmploymentStartDate,
            request.BaseSalary,
            request.Initials,
            request.CallingName,
            request.ProbationEndDate,
            request.ConfirmationDate,
            createdBy);

        await _dbContext.Employees.AddAsync(employee, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(employee);
    }

    public async Task UpdateAsync(Guid id, UpdateEmployeeRequestDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        await EnsureEmployeeCodeIsUniqueAsync(request.EmployeeCode, cancellationToken, id);
        await EnsureNicNumberIsUniqueAsync(request.NicNumber, cancellationToken, id);

        var modifiedBy = _currentUserService.UserName ?? "system";

        employee.Update(
            request.EmployeeCode,
            request.FirstName,
            request.LastName,
            request.NicNumber,
            request.DateOfBirth,
            request.Gender,
            request.MaritalStatus,
            request.EmploymentStartDate,
            request.BaseSalary,
            request.Initials,
            request.CallingName,
            request.ProbationEndDate,
            request.ConfirmationDate,
            modifiedBy);

        employee.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        var modifiedBy = _currentUserService.UserName ?? "system";
        employee.SoftDelete(modifiedBy);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureEmployeeCodeIsUniqueAsync(string employeeCode, CancellationToken cancellationToken, Guid? id = null)
    {
        var normalizedCode = employeeCode.Trim();
        var exists = await _dbContext.Employees.AnyAsync(
            e => e.EmployeeCode == normalizedCode && (!id.HasValue || e.Id != id.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Employee code must be unique.");
        }
    }

    private async Task EnsureNicNumberIsUniqueAsync(string nicNumber, CancellationToken cancellationToken, Guid? id = null)
    {
        var normalizedNic = nicNumber.Trim();
        var exists = await _dbContext.Employees.AnyAsync(
            e => e.NicNumber == normalizedNic && (!id.HasValue || e.Id != id.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("NIC number must be unique.");
        }
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Initials = employee.Initials,
            CallingName = employee.CallingName,
            NicNumber = employee.NicNumber,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            MaritalStatus = employee.MaritalStatus,
            EmploymentStartDate = employee.EmploymentStartDate,
            ProbationEndDate = employee.ProbationEndDate,
            ConfirmationDate = employee.ConfirmationDate,
            BaseSalary = employee.BaseSalary,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            CreatedBy = employee.CreatedBy,
            ModifiedAt = employee.ModifiedAt,
            ModifiedBy = employee.ModifiedBy
        };
    }
}

// TODO: add EmployeeService unit tests in a dedicated test project.
