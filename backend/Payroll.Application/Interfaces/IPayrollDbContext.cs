using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Employees;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<OvertimeRecord> OvertimeRecords { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
