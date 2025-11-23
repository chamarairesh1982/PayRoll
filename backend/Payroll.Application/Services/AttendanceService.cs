using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Attendance;
using Payroll.Domain.ValueObjects;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IPayrollDbContext _dbContext;

    public AttendanceService(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<AttendanceDto>> GetAttendanceAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AttendanceRecords
            .AsNoTracking()
            .OrderByDescending(r => r.Period.Start)
            .ThenBy(r => r.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AttendanceDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                HoursWorked = r.HoursWorked,
                PeriodStart = r.Period.Start,
                PeriodEnd = r.Period.End
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AttendanceDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task RecordAttendanceAsync(AttendanceDto attendance, CancellationToken cancellationToken = default)
    {
        if (attendance.PeriodStart > attendance.PeriodEnd)
        {
            throw new ArgumentException("PeriodStart must be before or equal to PeriodEnd");
        }

        var record = new AttendanceRecord
        {
            EmployeeId = attendance.EmployeeId,
            HoursWorked = attendance.HoursWorked,
            Period = new DateRange(attendance.PeriodStart, attendance.PeriodEnd)
        };

        await _dbContext.AttendanceRecords.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
