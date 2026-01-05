using FluentAssertions;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Application.Tests.TestInfrastructure;
using Xunit;

namespace Payroll.Application.Tests.PayrollConfig;

public class TaxRuleSetServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Reject_Overlapping_Slabs()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = TestPayrollDbContextFactory.Create(dbName);
        var service = new TaxRuleSetService(context, new TestCurrentUserService());

        var request = new CreateTaxRuleSetRequest
        {
            Name = "Overlap Test",
            YearOfAssessment = 2025,
            EffectiveFrom = new DateTime(2025, 4, 1),
            IsDefault = true,
            Slabs = new List<CreateTaxSlabItem>
            {
                new() { FromAmount = 0m, ToAmount = 100_000m, Rate = 0.05m, Order = 1 },
                new() { FromAmount = 90_000m, ToAmount = 150_000m, Rate = 0.10m, Order = 2 }
            }
        };

        var action = async () => await service.CreateAsync(request);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
