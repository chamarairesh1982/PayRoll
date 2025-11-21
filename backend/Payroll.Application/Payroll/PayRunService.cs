using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.Payroll.DTOs;
using Payroll.Application.Payroll.Requests;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;
using Payroll.Shared;

namespace Payroll.Application.Payroll;

public class PayRunService : IPayRunService
{
    private readonly IPayrollDbContext _dbContext;

    public PayRunService(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(int page, int pageSize, PayRunStatus? status)
    {
        var query = _dbContext.PayRuns.Include(pr => pr.PaySlips).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(pr => pr.Status == status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(pr => pr.PeriodStart)
            .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Select(pr => new PayRunSummaryDto
            {
                Id = pr.Id,
                Code = pr.Code,
                Name = pr.Name,
                PeriodType = pr.PeriodType,
                PeriodStart = pr.PeriodStart.ToDateTime(TimeOnly.MinValue),
                PeriodEnd = pr.PeriodEnd.ToDateTime(TimeOnly.MinValue),
                PayDate = pr.PayDate.ToDateTime(TimeOnly.MinValue),
                Status = pr.Status,
                IsLocked = pr.IsLocked,
                EmployeeCount = pr.PaySlips.Count,
                TotalNetPay = pr.PaySlips.Sum(ps => ps.NetPay)
            })
            .ToListAsync();

        return new PaginatedResult<PayRunSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PayRunDetailDto?> GetPayRunAsync(Guid id)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .FirstOrDefaultAsync(pr => pr.Id == id);

        if (payRun == null)
        {
            return null;
        }

        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).Distinct().ToList();
        var employees = await _dbContext.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        return MapPayRunDetail(payRun, employees);
    }

    public async Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request)
    {
        var periodStart = DateOnly.FromDateTime(request.PeriodStart);
        var periodEnd = DateOnly.FromDateTime(request.PeriodEnd);
        var payDate = DateOnly.FromDateTime(request.PayDate);

        var sequence = await _dbContext.PayRuns
            .CountAsync(pr => pr.PeriodStart.Year == periodStart.Year && pr.PeriodStart.Month == periodStart.Month) + 1;
        var code = $"PR-{periodStart:yyyyMM}-{sequence:D3}";

        var employeesQuery = _dbContext.Employees.AsQueryable();
        if (request.IncludeActiveEmployeesOnly)
        {
            employeesQuery = employeesQuery.Where(e => e.IsActive);
        }

        if (request.EmployeeIds is { Count: > 0 })
        {
            employeesQuery = employeesQuery.Where(e => request.EmployeeIds.Contains(e.Id));
        }

        var employees = await employeesQuery.ToListAsync();

        var payRun = new PayRun
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name,
            PeriodType = request.PeriodType,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PayDate = payDate,
            Status = PayRunStatus.Draft,
            IsLocked = false
        };

        foreach (var employee in employees)
        {
            var basic = employee.BaseSalary;
            var paySlip = new PaySlip
            {
                Id = Guid.NewGuid(),
                PayRun = payRun,
                EmployeeId = employee.Id,
                BasicSalary = basic,
                TotalEarnings = basic,
                TotalDeductions = 0m,
                NetPay = basic,
                EmployeeEpf = 0m,
                EmployerEpf = 0m,
                EmployerEtf = 0m,
                PayeTax = 0m,
                Currency = "LKR"
            };

            paySlip.Earnings.Add(new PaySlipEarningLine
            {
                Id = Guid.NewGuid(),
                PaySlip = paySlip,
                Code = "BASIC",
                Description = "Basic Salary",
                Amount = basic,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true
            });

            // TODO: Integrate attendance, overtime, loans, allowances/deductions, EPF/ETF, and tax calculations.

            payRun.PaySlips.Add(paySlip);
        }

        _dbContext.PayRuns.Add(payRun);
        await _dbContext.SaveChangesAsync();

        var employeeDictionary = employees.ToDictionary(e => e.Id);
        return MapPayRunDetail(payRun, employeeDictionary);
    }

    public async Task RecalculatePayRunAsync(Guid payRunId, RecalculatePayRunRequest request)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .FirstOrDefaultAsync(pr => pr.Id == payRunId);

        if (payRun == null)
        {
            throw new KeyNotFoundException("Pay run not found.");
        }

        if (payRun.Status is PayRunStatus.Posted or PayRunStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot recalculate a posted or cancelled pay run.");
        }

        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).Distinct().ToList();
        var employees = await _dbContext.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        foreach (var paySlip in payRun.PaySlips)
        {
            _dbContext.PaySlipEarningLines.RemoveRange(paySlip.Earnings);
            _dbContext.PaySlipDeductionLines.RemoveRange(paySlip.Deductions);
            paySlip.Earnings.Clear();
            paySlip.Deductions.Clear();

            var basic = employees.TryGetValue(paySlip.EmployeeId, out var employee)
                ? employee.BaseSalary
                : paySlip.BasicSalary;

            paySlip.BasicSalary = basic;
            paySlip.EmployeeEpf = 0m; // TODO: Apply Sri Lanka EPF rules.
            paySlip.EmployerEpf = 0m; // TODO: Apply Sri Lanka EPF rules.
            paySlip.EmployerEtf = 0m; // TODO: Apply Sri Lanka ETF rules.
            paySlip.PayeTax = 0m; // TODO: Apply PAYE calculations.

            paySlip.Earnings.Add(new PaySlipEarningLine
            {
                Id = Guid.NewGuid(),
                PaySlip = paySlip,
                Code = "BASIC",
                Description = "Basic Salary",
                Amount = basic,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true
            });

            // TODO: Integrate attendance, overtime, loans, allowances/deductions, EPF/ETF, and tax calculations.

            paySlip.TotalEarnings = paySlip.Earnings.Sum(l => l.Amount);
            paySlip.TotalDeductions = paySlip.Deductions.Sum(l => l.Amount);
            paySlip.NetPay = paySlip.TotalEarnings - paySlip.TotalDeductions;
        }

        payRun.Status = PayRunStatus.Calculated;

        await _dbContext.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(Guid payRunId, ChangePayRunStatusRequest request)
    {
        var payRun = await _dbContext.PayRuns.FirstOrDefaultAsync(pr => pr.Id == payRunId);
        if (payRun == null)
        {
            throw new KeyNotFoundException("Pay run not found.");
        }

        if (!IsTransitionAllowed(payRun.Status, request.Status))
        {
            throw new InvalidOperationException($"Invalid status transition from {payRun.Status} to {request.Status}.");
        }

        if (payRun.Status == PayRunStatus.Posted)
        {
            throw new InvalidOperationException("Posted pay runs are locked.");
        }

        payRun.Status = request.Status;
        if (request.Status == PayRunStatus.Posted)
        {
            payRun.IsLocked = true;
            // TODO: Lock related attendance/OT/loans entries when posting.
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<PaySlipDto?> GetPaySlipAsync(Guid payRunId, Guid paySlipId)
    {
        var paySlip = await _dbContext.PaySlips
            .Include(ps => ps.Earnings)
            .Include(ps => ps.Deductions)
            .Include(ps => ps.PayRun)
            .FirstOrDefaultAsync(ps => ps.Id == paySlipId && ps.PayRunId == payRunId);

        if (paySlip == null)
        {
            return null;
        }

        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == paySlip.EmployeeId);
        var employeeDictionary = employee != null
            ? new Dictionary<Guid, Employee> { { employee.Id, employee } }
            : new Dictionary<Guid, Employee>();

        return MapPaySlip(paySlip, employeeDictionary);
    }

    private static PayRunDetailDto MapPayRunDetail(PayRun payRun, IReadOnlyDictionary<Guid, Employee> employees)
    {
        var detail = new PayRunDetailDto
        {
            Id = payRun.Id,
            Code = payRun.Code,
            Name = payRun.Name,
            PeriodType = payRun.PeriodType,
            PeriodStart = payRun.PeriodStart.ToDateTime(TimeOnly.MinValue),
            PeriodEnd = payRun.PeriodEnd.ToDateTime(TimeOnly.MinValue),
            PayDate = payRun.PayDate.ToDateTime(TimeOnly.MinValue),
            Status = payRun.Status,
            IsLocked = payRun.IsLocked,
            EmployeeCount = payRun.PaySlips.Count,
            TotalNetPay = payRun.PaySlips.Sum(ps => ps.NetPay)
        };

        detail.PaySlips.AddRange(payRun.PaySlips.Select(ps => MapPaySlip(ps, employees)));
        return detail;
    }

    private static PaySlipDto MapPaySlip(PaySlip paySlip, IReadOnlyDictionary<Guid, Employee> employees)
    {
        employees.TryGetValue(paySlip.EmployeeId, out var employee);
        return new PaySlipDto
        {
            Id = paySlip.Id,
            PayRunId = paySlip.PayRunId,
            EmployeeId = paySlip.EmployeeId,
            EmployeeCode = employee?.EmployeeCode,
            EmployeeName = employee is null ? null : $"{employee.FirstName} {employee.LastName}",
            BasicSalary = paySlip.BasicSalary,
            TotalEarnings = paySlip.TotalEarnings,
            TotalDeductions = paySlip.TotalDeductions,
            NetPay = paySlip.NetPay,
            EmployeeEpf = paySlip.EmployeeEpf,
            EmployerEpf = paySlip.EmployerEpf,
            EmployerEtf = paySlip.EmployerEtf,
            PayeTax = paySlip.PayeTax,
            Currency = paySlip.Currency,
            Earnings = paySlip.Earnings.Select(e => new PaySlipEarningLineDto
            {
                Id = e.Id,
                Code = e.Code,
                Description = e.Description,
                Amount = e.Amount,
                IsEpfApplicable = e.IsEpfApplicable,
                IsEtfApplicable = e.IsEtfApplicable,
                IsTaxable = e.IsTaxable
            }).ToList(),
            Deductions = paySlip.Deductions.Select(d => new PaySlipDeductionLineDto
            {
                Id = d.Id,
                Code = d.Code,
                Description = d.Description,
                Amount = d.Amount,
                IsPreTax = d.IsPreTax,
                IsPostTax = d.IsPostTax
            }).ToList()
        };
    }

    private static bool IsTransitionAllowed(PayRunStatus current, PayRunStatus target)
    {
        if (current == target)
        {
            return true;
        }

        return (current, target) switch
        {
            (PayRunStatus.Draft, PayRunStatus.Calculated) => true,
            (PayRunStatus.Calculated, PayRunStatus.UnderReview) => true,
            (PayRunStatus.UnderReview, PayRunStatus.Approved) => true,
            (PayRunStatus.Approved, PayRunStatus.Posted) => true,
            (PayRunStatus.Calculated, PayRunStatus.Draft) => true,
            (PayRunStatus.UnderReview, PayRunStatus.Draft) => true,
            (_, PayRunStatus.Cancelled) when current != PayRunStatus.Posted => true,
            _ => false
        };
    }
}
