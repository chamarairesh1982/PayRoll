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
    private readonly IAuditLogger _auditLogger;

    public EmployeeService(IPayrollDbContext dbContext, ICurrentUserService currentUserService, IAuditLogger auditLogger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
    }

    public async Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(
        int page,
        int pageSize,
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.IsActive);

        if (companyId.HasValue)
        {
            query = query.Where(e => e.CompanyId == companyId);
        }

        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId);
        }

        if (costCenterId.HasValue)
        {
            query = query.Where(e => e.CostCenterId == costCenterId);
        }

        query = query.OrderBy(e => e.EmployeeCode);

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
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            request.Initials,
            request.CallingName,
            request.ProbationEndDate,
            request.ConfirmationDate,
            createdBy,
            request.BankName,
            request.BankCode,
            request.BranchCode,
            request.BankAccountNumber);

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

        var salaryChanged = employee.BaseSalary != request.BaseSalary;
        var salaryBefore = salaryChanged ? CreateSalarySnapshot(employee) : null;

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
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            request.Initials,
            request.CallingName,
            request.ProbationEndDate,
            request.ConfirmationDate,
            modifiedBy,
            request.BankName,
            request.BankCode,
            request.BranchCode,
            request.BankAccountNumber);

        employee.IsActive = request.IsActive;

        if (salaryChanged)
        {
            var salaryAfter = CreateSalarySnapshot(employee);
            await _auditLogger.LogAsync(
                nameof(Employee),
                employee.Id.ToString(),
                "SalaryUpdated",
                salaryBefore,
                salaryAfter,
                modifiedBy,
                cancellationToken);
        }

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

    private static object CreateSalarySnapshot(Employee employee)
    {
        return new
        {
            employee.Id,
            employee.EmployeeCode,
            employee.BaseSalary,
            employee.ModifiedAt,
            employee.ModifiedBy
        };
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
            CompanyId = employee.CompanyId,
            BranchId = employee.BranchId,
            CostCenterId = employee.CostCenterId,
            BankName = employee.BankName,
            BankCode = employee.BankCode,
            BranchCode = employee.BranchCode,
            BankAccountNumber = employee.BankAccountNumber,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            CreatedBy = employee.CreatedBy,
            ModifiedAt = employee.ModifiedAt,
            ModifiedBy = employee.ModifiedBy
        };
    }
}

// TODO: add EmployeeService unit tests in a dedicated test project.
