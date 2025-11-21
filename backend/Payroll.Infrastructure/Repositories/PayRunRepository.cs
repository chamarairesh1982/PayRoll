using Payroll.Domain.Payroll;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure.Repositories;

public class PayRunRepository : GenericRepository<PayRun>
{
    public PayRunRepository(PayrollDbContext context) : base(context)
    {
    }
}
