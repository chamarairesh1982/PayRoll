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
            .Join(
                _dbContext.Employees.AsNoTracking(),
                record => record.EmployeeId,
                employee => employee.Id,
                (record, employee) => new { record, employee })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(result => new AttendanceDto
            {
                Id = result.record.Id,
                EmployeeId = result.record.EmployeeId,
                EmployeeName = (result.employee.FirstName + " " + result.employee.LastName).Trim(),
                HoursWorked = result.record.HoursWorked,
                PeriodStart = result.record.Period.Start,
                PeriodEnd = result.record.Period.End
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
