using FluentAssertions;
using Payroll.Application.PayrollConfig;
using Payroll.Application.Tests.TestInfrastructure;
using Payroll.Domain.PayrollConfig;
using Xunit;

namespace Payroll.Application.Tests.Services;

public class EpfEtfRuleSetServiceTests
{
    [Fact]
    public async Task GetActiveRuleForDateAsync_Should_Pick_Latest_Effective_Rule()
    {
        using var context = new TestContext();
        var service = new EpfEtfRuleSetService(context.DbContext, context.CurrentUserService);

        context.DbContext.EpfEtfRuleSets.AddRange(new[]
        {
            new EpfEtfRuleSet
            {
                Id = Guid.NewGuid(),
                Name = "Older Rule",
                EffectiveFrom = new DateOnly(2024, 1, 1),
                EmployeeEpfRate = 8m,
                EmployerEpfRate = 12m,
                EmployerEtfRate = 3m,
                IsDefault = false,
                IsActive = true,
                CreatedBy = "seed"
            },
            new EpfEtfRuleSet
            {
                Id = Guid.NewGuid(),
                Name = "New Rule",
                EffectiveFrom = new DateOnly(2025, 1, 1),
                EmployeeEpfRate = 7m,
                EmployerEpfRate = 12m,
                EmployerEtfRate = 3m,
                IsDefault = true,
                IsActive = true,
                CreatedBy = "seed"
            }
        });

        await context.DbContext.SaveChangesAsync();

        var selected = await service.GetActiveRuleForDateAsync(new DateOnly(2025, 2, 1));

        selected.Should().NotBeNull();
        selected!.Name.Should().Be("New Rule");
        selected.EmployeeEpfRate.Should().Be(7m);
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
