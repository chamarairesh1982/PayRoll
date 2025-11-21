using Payroll.Domain.Employees;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>
{
    public EmployeeRepository(PayrollDbContext context) : base(context)
    {
    }
}
