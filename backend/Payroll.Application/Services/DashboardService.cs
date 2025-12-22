using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Employees;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IPayrollDbContext _dbContext;

    public DashboardService(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        DashboardSummaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var summary = new DashboardSummaryDto();

        try
        {
            var employeeQuery = ApplyEmployeeFilters(_dbContext.Employees.AsNoTracking().Where(e => e.IsActive), request);
            summary.Kpis.EmployeeCount = await SafeCountAsync(employeeQuery, cancellationToken);

            summary.Health.MissingEpf = await SafeCountAsync(
                employeeQuery.Where(e => string.IsNullOrWhiteSpace(e.EpfNumber)),
                cancellationToken);

            summary.Health.MissingBank = await SafeCountAsync(
                employeeQuery.Where(e =>
                    string.IsNullOrWhiteSpace(e.BankAccountNumber)
                    || string.IsNullOrWhiteSpace(e.BankName)
                    || string.IsNullOrWhiteSpace(e.BankCode)
                    || string.IsNullOrWhiteSpace(e.BranchCode)),
                cancellationToken);

            var payRunQuery = ApplyPayRunFilters(_dbContext.PayRuns.AsNoTracking(), request);

            summary.Kpis.NextPayDate = await SafeFirstOrDefaultAsync(
                payRunQuery
                    .Where(pr => pr.PayDate >= DateTime.UtcNow.Date)
                    .OrderBy(pr => pr.PayDate)
                    .Select(pr => (DateTime?)pr.PayDate),
                cancellationToken);

            var latestPayRun = await SafeFirstOrDefaultAsync(
                payRunQuery
                    .OrderByDescending(pr => pr.PayDate)
                    .Select(pr => new { pr.Status }),
                cancellationToken);

            summary.Kpis.LatestPayRunStatus = latestPayRun is null
                ? string.Empty
                : latestPayRun.Status.ToString();

            summary.Health.NegativeNet = await SafeCountAsync(
                from payslip in _dbContext.PaySlips.AsNoTracking()
                join payRun in payRunQuery on payslip.PayRunId equals payRun.Id
                where payslip.NetPay < 0
                select payslip.Id,
                cancellationToken);

            if (request.PeriodStart.HasValue && request.PeriodEnd.HasValue)
            {
                var periodStart = DateOnly.FromDateTime(request.PeriodStart.Value);
                var periodEnd = DateOnly.FromDateTime(request.PeriodEnd.Value);

                summary.Health.PendingAttendance = await ResolvePendingAttendanceAsync(
                    employeeQuery,
                    periodStart,
                    periodEnd,
                    cancellationToken);

                summary.Health.PendingOt = await ResolvePendingOtAsync(
                    request,
                    periodStart,
                    periodEnd,
                    cancellationToken);
            }

            summary.Kpis.ExceptionsCount = summary.Health.MissingEpf
                + summary.Health.MissingBank
                + summary.Health.PendingAttendance
                + summary.Health.PendingOt
                + summary.Health.NegativeNet;
        }
        catch
        {
            return summary;
        }

        return summary;
    }

    private async Task<int> ResolvePendingAttendanceAsync(
        IQueryable<Employee> employeeQuery,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeesWithAttendance = await _dbContext.AttendanceRecords
                .AsNoTracking()
                .Where(record => record.Period.Start <= periodEnd && record.Period.End >= periodStart)
                .Select(record => record.EmployeeId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var scopedEmployeeIds = await employeeQuery.Select(e => e.Id).ToListAsync(cancellationToken);
            var attendanceCount = employeesWithAttendance.Intersect(scopedEmployeeIds).Count();
            var missing = scopedEmployeeIds.Count - attendanceCount;
            return Math.Max(missing, 0);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> ResolvePendingOtAsync(
        DashboardSummaryRequest request,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = from ot in _dbContext.OTEntries.AsNoTracking()
                        join employee in _dbContext.Employees.AsNoTracking() on ot.EmployeeId equals employee.Id
                        where ot.Status == OvertimeStatus.Submitted
                              && ot.Date >= periodStart
                              && ot.Date <= periodEnd
                              && employee.IsActive
                        select new { ot.Id, employee.CompanyId, employee.BranchId, employee.CostCenterId };

            if (request.CompanyId.HasValue)
            {
                query = query.Where(item => item.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue)
            {
                query = query.Where(item => item.BranchId == request.BranchId);
            }

            if (request.CostCenterId.HasValue)
            {
                query = query.Where(item => item.CostCenterId == request.CostCenterId);
            }

            return await query.CountAsync(cancellationToken);
        }
        catch
        {
            return 0;
        }
    }

    private static IQueryable<Employee> ApplyEmployeeFilters(
        IQueryable<Employee> query,
        DashboardSummaryRequest request)
    {
        if (request.CompanyId.HasValue)
        {
            query = query.Where(e => e.CompanyId == request.CompanyId);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(e => e.BranchId == request.BranchId);
        }

        if (request.CostCenterId.HasValue)
        {
            query = query.Where(e => e.CostCenterId == request.CostCenterId);
        }

        return query;
    }

    private static IQueryable<PayRun> ApplyPayRunFilters(
        IQueryable<PayRun> query,
        DashboardSummaryRequest request)
    {
        if (request.CompanyId.HasValue)
        {
            query = query.Where(pr => pr.CompanyId == request.CompanyId);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(pr => pr.BranchId == request.BranchId);
        }

        if (request.CostCenterId.HasValue)
        {
            query = query.Where(pr => pr.CostCenterId == request.CostCenterId);
        }

        if (request.PeriodStart.HasValue)
        {
            query = query.Where(pr => pr.PeriodEnd >= request.PeriodStart.Value);
        }

        if (request.PeriodEnd.HasValue)
        {
            query = query.Where(pr => pr.PeriodStart <= request.PeriodEnd.Value);
        }

        return query;
    }

    private static async Task<int> SafeCountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken)
    {
        try
        {
            return await query.CountAsync(cancellationToken);
        }
        catch
        {
            return 0;
        }
    }

    private static async Task<T?> SafeFirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken)
    {
        try
        {
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch
        {
            return default;
        }
    }
}
