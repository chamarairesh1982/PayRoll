using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Employees;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
