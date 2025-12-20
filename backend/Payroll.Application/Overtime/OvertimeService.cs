using Microsoft.EntityFrameworkCore;
using Payroll.Application.Exceptions;
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

    public async Task<PaginatedResult<OTEntryDto>> GetAsync(
        int page,
        int pageSize,
        Guid? employeeId,
        DateOnly? from,
        DateOnly? to,
        OvertimeStatus? status)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.OTEntries
            .AsNoTracking()
            .Where(o => o.IsActive)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(o => o.EmployeeId == employeeId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(o => o.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(o => o.Date <= to.Value);
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
            .Distinct()
            .ToList();

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e);

        var items = records.Select(o => MapToDto(o, employees)).ToList();

        return new PaginatedResult<OTEntryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OTEntryDto?> GetByIdAsync(Guid id)
    {
        var overtime = await _dbContext.OTEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (overtime is null)
        {
            return null;
        }

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.Id == overtime.EmployeeId)
            .ToDictionaryAsync(e => e.Id, e => e);

        return MapToDto(overtime, employees);
    }

    public async Task<OTEntryDto> CreateAsync(CreateOTEntryRequest request)
    {
        EnsureRole("Maker", "HR");
        EnsureValidStatus(request.Status);

        if (request.Status is not (OvertimeStatus.Draft or OvertimeStatus.Submitted))
        {
            throw new InvalidOperationException("Overtime entries can only be created as draft or submitted.");
        }

        if (request.RawMinutes <= 0)
        {
            throw new ArgumentException("Minutes must be greater than zero.", nameof(request.RawMinutes));
        }

        var employee = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
        if (employee is null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        var overtime = new OTEntry
        {
            EmployeeId = request.EmployeeId,
            Date = DateOnly.FromDateTime(request.WorkDate.Date),
            RawMinutes = request.RawMinutes,
            Type = request.Type,
            Status = request.Status,
            Comment = request.Comment?.Trim(),
            ApprovedAtUtc = null,
            ApprovedByUserId = null,
            IsLockedForPayroll = false,
            CreatedByUserId = _currentUserService.UserId,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.OTEntries.AddAsync(overtime);
        await _dbContext.SaveChangesAsync();

        var employees = new Dictionary<Guid, Domain.Employees.Employee>
        {
            { employee.Id, employee }
        };

        return MapToDto(overtime, employees);
    }

    public async Task UpdateAsync(Guid id, UpdateOTEntryRequest request)
    {
        var overtime = await _dbContext.OTEntries.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
        if (overtime is null)
        {
            throw new KeyNotFoundException("Overtime record not found");
        }

        EnsureEditable(overtime);

        if (overtime.IsLockedForPayroll)
        {
            throw new InvalidOperationException("Overtime record is locked for payroll and cannot be modified.");
        }

        if (request.WorkDate.HasValue)
        {
            overtime.Date = DateOnly.FromDateTime(request.WorkDate.Value.Date);
        }

        if (request.RawMinutes.HasValue)
        {
            if (request.RawMinutes.Value <= 0)
            {
                throw new ArgumentException("Minutes must be greater than zero.", nameof(request.RawMinutes));
            }

            overtime.RawMinutes = request.RawMinutes.Value;
        }

        if (request.Type.HasValue)
        {
            overtime.Type = request.Type.Value;
        }

        if (request.Comment != null)
        {
            overtime.Comment = request.Comment.Trim();
        }

        overtime.ModifiedAt = DateTime.UtcNow;
        overtime.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task SubmitAsync(Guid id)
    {
        var overtime = await _dbContext.OTEntries.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
        if (overtime is null)
        {
            throw new KeyNotFoundException("Overtime record not found");
        }

        EnsureEditable(overtime);

        if (overtime.IsLockedForPayroll)
        {
            throw new InvalidOperationException("Overtime record is locked for payroll and cannot be submitted.");
        }

        if (overtime.Status != OvertimeStatus.Draft)
        {
            throw new InvalidOperationException("Only draft overtime entries can be submitted.");
        }

        overtime.Status = OvertimeStatus.Submitted;
        overtime.ModifiedAt = DateTime.UtcNow;
        overtime.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task ApproveAsync(Guid id, OvertimeActionRequest request)
    {
        EnsureRole("Approver");

        var overtime = await _dbContext.OTEntries.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
        if (overtime is null)
        {
            throw new KeyNotFoundException("Overtime record not found");
        }

        if (overtime.IsLockedForPayroll)
        {
            throw new InvalidOperationException("Overtime record is locked for payroll and cannot be approved.");
        }

        if (overtime.Status != OvertimeStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted overtime entries can be approved.");
        }

        overtime.Status = OvertimeStatus.Approved;
        overtime.Comment = request.Comment?.Trim() ?? overtime.Comment;
        overtime.ApprovedAtUtc = DateTimeOffset.UtcNow;
        overtime.ApprovedByUserId = _currentUserService.UserId;
        overtime.ModifiedAt = DateTime.UtcNow;
        overtime.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid id, OvertimeActionRequest request)
    {
        EnsureRole("Approver");

        var overtime = await _dbContext.OTEntries.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
        if (overtime is null)
        {
            throw new KeyNotFoundException("Overtime record not found");
        }

        if (overtime.IsLockedForPayroll)
        {
            throw new InvalidOperationException("Overtime record is locked for payroll and cannot be rejected.");
        }

        if (overtime.Status != OvertimeStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted overtime entries can be rejected.");
        }

        overtime.Status = OvertimeStatus.Rejected;
        overtime.Comment = request.Comment?.Trim() ?? overtime.Comment;
        overtime.ApprovedAtUtc = DateTimeOffset.UtcNow;
        overtime.ApprovedByUserId = _currentUserService.UserId;
        overtime.ModifiedAt = DateTime.UtcNow;
        overtime.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static OTEntryDto MapToDto(
        OTEntry overtime,
        IReadOnlyDictionary<Guid, Domain.Employees.Employee> employees)
    {
        employees.TryGetValue(overtime.EmployeeId, out var employee);
        return new OTEntryDto
        {
            Id = overtime.Id,
            EmployeeId = overtime.EmployeeId,
            EmployeeCode = employee?.EmployeeCode,
            EmployeeName = employee is null ? null : $"{employee.FirstName} {employee.LastName}",
            WorkDate = overtime.Date.ToDateTime(TimeOnly.MinValue),
            RawMinutes = overtime.RawMinutes,
            Type = overtime.Type,
            Status = overtime.Status,
            Comment = overtime.Comment,
            ApprovedByUserId = overtime.ApprovedByUserId,
            ApprovedAtUtc = overtime.ApprovedAtUtc?.UtcDateTime,
            CreatedByUserId = overtime.CreatedByUserId,
            CreatedAtUtc = overtime.CreatedAt,
            UpdatedAtUtc = overtime.ModifiedAt,
            PayRunId = overtime.PayRunId,
            IsLockedForPayroll = overtime.IsLockedForPayroll
        };
    }

    private void EnsureRole(params string[] roles)
    {
        var hasRole = _currentUserService.Roles.Any(role =>
            roles.Any(r => string.Equals(role, r, StringComparison.OrdinalIgnoreCase)));
        if (!hasRole)
        {
            throw new ForbiddenAccessException($"Only users with roles {string.Join(" or ", roles)} can perform this action.");
        }
    }

    private void EnsureEditable(OTEntry entry)
    {
        var isCreator = !string.IsNullOrWhiteSpace(_currentUserService.UserId)
            && string.Equals(entry.CreatedByUserId, _currentUserService.UserId, StringComparison.OrdinalIgnoreCase);

        var isHrOrMaker = _currentUserService.Roles.Any(role =>
            string.Equals(role, "Maker", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "HR", StringComparison.OrdinalIgnoreCase));

        if (!isCreator && !isHrOrMaker)
        {
            throw new ForbiddenAccessException("Only the entry creator or HR users can modify this overtime entry.");
        }

        if (entry.Status != OvertimeStatus.Draft)
        {
            throw new InvalidOperationException("Only draft overtime entries can be edited.");
        }
    }

    private static void EnsureValidStatus(OvertimeStatus status)
    {
        if (!Enum.IsDefined(typeof(OvertimeStatus), status))
        {
            throw new ArgumentException("Invalid overtime status.");
        }
    }
}
