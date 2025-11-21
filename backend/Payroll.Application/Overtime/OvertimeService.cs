using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.Overtime.DTOs;
using Payroll.Domain.Overtime;
using Payroll.Shared;

namespace Payroll.Application.Overtime;

public class OvertimeService : IOvertimeService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public OvertimeService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<OvertimeRecordDto>> GetAsync(
        int page,
        int pageSize,
        Guid? employeeId,
        DateOnly? date,
        OvertimeStatus? status)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.OvertimeRecords
            .AsNoTracking()
            .Where(o => o.IsActive)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(o => o.EmployeeId == employeeId.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(o => o.Date == date.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var records = await query
            .OrderByDescending(o => o.Date)
            .ThenByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var employeeIds = records
            .Select(o => o.EmployeeId)
            .Concat(records.Where(o => o.ApprovedById.HasValue).Select(o => o.ApprovedById!.Value))
            .Distinct()
            .ToList();

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e);

        var items = records.Select(o => MapToDto(o, employees)).ToList();

        return new PaginatedResult<OvertimeRecordDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OvertimeRecordDto?> GetByIdAsync(Guid id)
    {
        var overtime = await _dbContext.OvertimeRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (overtime is null)
        {
            return null;
        }

        var employeeIds = new List<Guid> { overtime.EmployeeId };
        if (overtime.ApprovedById.HasValue)
        {
            employeeIds.Add(overtime.ApprovedById.Value);
        }

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e);

        return MapToDto(overtime, employees);
    }

    public async Task<OvertimeRecordDto> CreateAsync(CreateOvertimeRecordRequest request)
    {
        if (request.Hours <= 0)
        {
            throw new ArgumentException("Hours must be greater than zero.", nameof(request.Hours));
        }

        var employee = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
        if (employee is null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        var overtime = new OvertimeRecord
        {
            EmployeeId = request.EmployeeId,
            Date = DateOnly.FromDateTime(request.Date.Date),
            Hours = request.Hours,
            Type = request.Type,
            Status = OvertimeStatus.Pending,
            Reason = request.Reason?.Trim(),
            ApprovedAt = null,
            ApprovedById = null,
            IsLockedForPayroll = false,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.OvertimeRecords.AddAsync(overtime);
        await _dbContext.SaveChangesAsync();

        var employees = new Dictionary<Guid, Domain.Employees.Employee>
        {
            { employee.Id, employee }
        };

        return MapToDto(overtime, employees);
    }

    public async Task UpdateAsync(Guid id, UpdateOvertimeRecordRequest request)
    {
        var overtime = await _dbContext.OvertimeRecords.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
        if (overtime is null)
        {
            throw new KeyNotFoundException("Overtime record not found");
        }

        if (overtime.IsLockedForPayroll)
        {
            throw new InvalidOperationException("Overtime record is locked for payroll and cannot be modified.");
        }

        if (request.Date.HasValue)
        {
            overtime.Date = DateOnly.FromDateTime(request.Date.Value.Date);
        }

        if (request.Hours.HasValue)
        {
            if (request.Hours.Value <= 0)
            {
                throw new ArgumentException("Hours must be greater than zero.", nameof(request.Hours));
            }

            overtime.Hours = request.Hours.Value;
        }

        if (request.Type.HasValue)
        {
            overtime.Type = request.Type.Value;
        }

        if (request.Reason != null)
        {
            overtime.Reason = request.Reason.Trim();
        }

        if (request.Status.HasValue)
        {
            overtime.Status = request.Status.Value;
            overtime.ApprovedAt = request.Status is OvertimeStatus.Approved or OvertimeStatus.Rejected
                ? DateTimeOffset.UtcNow
                : null;
            overtime.ApprovedById = overtime.ApprovedAt.HasValue && Guid.TryParse(_currentUserService.UserId, out var approverId)
                ? approverId
                : null;
        }

        overtime.ModifiedAt = DateTime.UtcNow;
        overtime.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var overtime = await _dbContext.OvertimeRecords.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
        if (overtime is null)
        {
            throw new KeyNotFoundException("Overtime record not found");
        }

        if (overtime.IsLockedForPayroll)
        {
            throw new InvalidOperationException("Overtime record is locked for payroll and cannot be deleted.");
        }

        overtime.IsActive = false;
        overtime.ModifiedAt = DateTime.UtcNow;
        overtime.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static OvertimeRecordDto MapToDto(
        OvertimeRecord overtime,
        IReadOnlyDictionary<Guid, Domain.Employees.Employee> employees)
    {
        employees.TryGetValue(overtime.EmployeeId, out var employee);
        var approvedBy = overtime.ApprovedById.HasValue && employees.TryGetValue(overtime.ApprovedById.Value, out var approver)
            ? approver
            : null;

        return new OvertimeRecordDto
        {
            Id = overtime.Id,
            EmployeeId = overtime.EmployeeId,
            EmployeeCode = employee?.EmployeeCode,
            EmployeeName = employee is null ? null : $"{employee.FirstName} {employee.LastName}",
            Date = overtime.Date.ToDateTime(TimeOnly.MinValue),
            Hours = overtime.Hours,
            Type = overtime.Type,
            Status = overtime.Status,
            Reason = overtime.Reason,
            ApprovedById = overtime.ApprovedById,
            ApprovedByName = approvedBy is null ? null : $"{approvedBy.FirstName} {approvedBy.LastName}",
            ApprovedAt = overtime.ApprovedAt?.UtcDateTime,
            IsLockedForPayroll = overtime.IsLockedForPayroll
        };
    }
}
