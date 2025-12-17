using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Domain.Employees;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<OvertimeRecord> OvertimeRecords { get; }
    DbSet<Loan> Loans { get; }
    DbSet<PayRun> PayRuns { get; }
    DbSet<PaySlip> PaySlips { get; }
    DbSet<PayRunApproval> PayRunApprovals { get; }
    DbSet<AllowanceType> AllowanceTypes { get; }
    DbSet<DeductionType> DeductionTypes { get; }
    DbSet<EpfEtfRuleSet> EpfEtfRuleSets { get; }
    DbSet<TaxRuleSet> TaxRuleSets { get; }
    DbSet<TaxSlab> TaxSlabs { get; }
    DbSet<EmployeePayItem> EmployeePayItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
