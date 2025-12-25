using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<PayRun> PayRuns { get; }
    DbSet<PayRunLineItem> PayRunLineItems { get; }
    DbSet<Payslip> Payslips { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
