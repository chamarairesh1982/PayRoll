using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.Leave.DTOs;
using Payroll.Domain.Leave;
using Payroll.Shared;

namespace Payroll.Application.Leave;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public LeaveRequestService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<LeaveRequestDto>> GetAsync(int page, int pageSize, Guid? employeeId, LeaveStatus? status)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(l => l.IsActive)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var leaves = await query
            .OrderByDescending(l => l.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var employeeIds = leaves
            .Select(l => l.EmployeeId)
            .Concat(leaves.Where(l => l.ApprovedById.HasValue).Select(l => l.ApprovedById!.Value))
            .Distinct()
            .ToList();

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e);

        var items = leaves.Select(l => MapToDto(l, employees)).ToList();

        return new PaginatedResult<LeaveRequestDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(Guid id)
    {
        var leave = await _dbContext.LeaveRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);

        if (leave is null)
        {
            return null;
        }

        var employeeIds = new List<Guid> { leave.EmployeeId };
        if (leave.ApprovedById.HasValue)
        {
            employeeIds.Add(leave.ApprovedById.Value);
        }

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e);

        return MapToDto(leave, employees);
    }

    public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestRequest request)
    {
        var employee = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
        if (employee is null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        var startDate = DateOnly.FromDateTime(request.StartDate.Date);
        var endDate = DateOnly.FromDateTime(request.EndDate.Date);

        EnsureDateRangeIsValid(startDate, endDate);

        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            StartDate = startDate,
            EndDate = endDate,
            IsHalfDay = request.IsHalfDay,
            HalfDaySession = request.IsHalfDay == true ? request.HalfDaySession?.Trim() : null,
            Reason = request.Reason?.Trim(),
            Status = LeaveStatus.Pending,
            RequestedAt = DateTimeOffset.UtcNow,
            TotalDays = CalculateTotalDays(startDate, endDate, request.IsHalfDay),
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.LeaveRequests.AddAsync(leave);
        await _dbContext.SaveChangesAsync();

        var employees = new Dictionary<Guid, Domain.Employees.Employee>
        {
            { employee.Id, employee }
        };

        return MapToDto(leave, employees);
    }

    public async Task UpdateAsync(Guid id, UpdateLeaveRequestRequest request)
    {
        var leave = await _dbContext.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id && l.IsActive);
        if (leave is null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != leave.EmployeeId)
        {
            var employeeExists = await _dbContext.Employees.AnyAsync(e => e.Id == request.EmployeeId.Value);
            if (!employeeExists)
            {
                throw new KeyNotFoundException("Employee not found");
            }

            leave.EmployeeId = request.EmployeeId.Value;
        }

        if (request.LeaveType.HasValue)
        {
            leave.LeaveType = request.LeaveType.Value;
        }

        var updatedStartDate = request.StartDate.HasValue ? DateOnly.FromDateTime(request.StartDate.Value.Date) : leave.StartDate;
        var updatedEndDate = request.EndDate.HasValue ? DateOnly.FromDateTime(request.EndDate.Value.Date) : leave.EndDate;

        EnsureDateRangeIsValid(updatedStartDate, updatedEndDate);

        leave.StartDate = updatedStartDate;
        leave.EndDate = updatedEndDate;

        if (request.IsHalfDay.HasValue)
        {
            leave.IsHalfDay = request.IsHalfDay;
        }

        if (request.HalfDaySession != null)
        {
            leave.HalfDaySession = request.HalfDaySession.Trim();
        }

        if (leave.IsHalfDay != true)
        {
            leave.HalfDaySession = null;
        }

        if (request.Reason != null)
        {
            leave.Reason = request.Reason.Trim();
        }

        if (request.Status.HasValue)
        {
            leave.Status = request.Status.Value;
            leave.ApprovedAt = request.Status is LeaveStatus.Approved or LeaveStatus.Rejected
                ? DateTimeOffset.UtcNow
                : leave.ApprovedAt;
        }

        if (request.ApprovedById.HasValue)
        {
            leave.ApprovedById = request.ApprovedById;
        }

        leave.TotalDays = CalculateTotalDays(leave.StartDate, leave.EndDate, leave.IsHalfDay);
        leave.ModifiedAt = DateTime.UtcNow;
        leave.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var leave = await _dbContext.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id && l.IsActive);
        if (leave is null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        leave.IsActive = false;
        leave.ModifiedAt = DateTime.UtcNow;
        leave.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static double CalculateTotalDays(DateOnly startDate, DateOnly endDate, bool? isHalfDay)
    {
        if (isHalfDay == true)
        {
            return 0.5;
        }

        return (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;
    }

    private static void EnsureDateRangeIsValid(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date.");
        }
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leave, IReadOnlyDictionary<Guid, Domain.Employees.Employee> employees)
    {
        employees.TryGetValue(leave.EmployeeId, out var employee);
        var approvedBy = leave.ApprovedById.HasValue && employees.TryGetValue(leave.ApprovedById.Value, out var approver)
            ? approver
            : null;

        return new LeaveRequestDto
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            EmployeeCode = employee?.EmployeeCode,
            EmployeeName = employee is null ? null : $"{employee.FirstName} {employee.LastName}",
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = leave.EndDate.ToDateTime(TimeOnly.MinValue),
            TotalDays = leave.TotalDays,
            Reason = leave.Reason,
            Status = leave.Status,
            ApprovedById = leave.ApprovedById,
            ApprovedByName = approvedBy is null ? null : $"{approvedBy.FirstName} {approvedBy.LastName}",
            RequestedAt = leave.RequestedAt.UtcDateTime,
            ApprovedAt = leave.ApprovedAt?.UtcDateTime,
            IsHalfDay = leave.IsHalfDay,
            HalfDaySession = leave.HalfDaySession
        };
    }
}
