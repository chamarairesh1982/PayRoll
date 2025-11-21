using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<PayRun> PayRuns { get; }
    DbSet<PaySlip> PaySlips { get; }
    DbSet<PaySlipEarningLine> PaySlipEarningLines { get; }
    DbSet<PaySlipDeductionLine> PaySlipDeductionLines { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<OvertimeRecord> OvertimeRecords { get; }
    DbSet<AllowanceType> AllowanceTypes { get; }
    DbSet<DeductionType> DeductionTypes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
