using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Employees;
using Payroll.Domain.Leave;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
