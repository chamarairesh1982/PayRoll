using FluentAssertions;
using Payroll.Application.Overtime;
using Payroll.Application.Overtime.DTOs;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Domain.Overtime;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class OvertimeServiceTests
{
    [Fact]
    public async Task Update_Should_Reject_When_Entry_Is_Locked_For_Payroll()
    {
        using var context = new TestContext();
        var employee = TestDataSeeder.SeedEmployee(context.DbContext, "EMP_LOCK", "Maya", 60_000m);

        var entry = new OTEntry
        {
            EmployeeId = employee.Id,
            Date = new DateOnly(2025, 4, 10),
            RawMinutes = 120,
            Type = OvertimeType.Normal,
            Status = OvertimeStatus.Draft,
            IsLockedForPayroll = true,
            CreatedBy = "seed",
            CreatedByUserId = "test-user"
        };

        context.DbContext.OTEntries.Add(entry);
        await context.DbContext.SaveChangesAsync();

        var service = new OvertimeService(context.DbContext, context.CurrentUserService);

        var act = () => service.UpdateAsync(entry.Id, new UpdateOTEntryRequest
        {
            RawMinutes = 180
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Overtime record is locked for payroll and cannot be modified.");
    }

    private sealed class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = TestPayrollDbContextFactory.Create(Guid.NewGuid().ToString());
            CurrentUserService = new TestCurrentUserService();
        }

        public Payroll.Infrastructure.Persistence.PayrollDbContext DbContext { get; }
        public TestCurrentUserService CurrentUserService { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
