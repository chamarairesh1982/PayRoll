using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Employees;
using Payroll.Infrastructure.Persistence;
using Payroll.Application.Tests.Common;
using Xunit;

namespace Payroll.Application.Tests;

public class TenantScopingTests
{
    [Fact]
    public async Task Should_Only_Return_Data_For_Current_Tenant()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext();
        
        using (var context = new ApplicationDbContext(options, tenantContext))
        {
            // Set tenant 1 and add data
            tenantContext.TenantId = tenant1;
            context.Employees.Add(new Employee { FirstName = "Tenant 1 Employee", EmployeeCode = "T1" });
            
            // Set tenant 2 and add data
            tenantContext.TenantId = tenant2;
            context.Employees.Add(new Employee { FirstName = "Tenant 2 Employee", EmployeeCode = "T2" });
            
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new ApplicationDbContext(options, tenantContext))
        {
            // Verify Tenant 1 scoping
            tenantContext.TenantId = tenant1;
            var t1Employees = await context.Employees.ToListAsync();
            Assert.Single(t1Employees);
            Assert.Equal("Tenant 1 Employee", t1Employees[0].FirstName);

            // Verify Tenant 2 scoping
            tenantContext.TenantId = tenant2;
            var t2Employees = await context.Employees.ToListAsync();
            Assert.Single(t2Employees);
            Assert.Equal("Tenant 2 Employee", t2Employees[0].FirstName);
        }
    }
}
