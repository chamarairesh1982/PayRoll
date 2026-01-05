using Microsoft.EntityFrameworkCore;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Application.Tests.TestInfrastructure;

public static class TestPayrollDbContextFactory
{
    public static PayrollDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PayrollDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new PayrollDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
