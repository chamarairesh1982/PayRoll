using Payroll.Application.DTOs.RulePackages;

namespace Payroll.Application.Interfaces;

public interface IRuleVersionResolver
{
    Task<RuleVersionResolutionResult> ResolveRuleVersionsAsync(Guid? companyId, DateOnly periodEndDate, CancellationToken cancellationToken = default);
    Task<RuleVersionResolutionResult> ResolveRuleVersionsByIdsAsync(Guid? taxVersionId, Guid? epfVersionId, Guid? etfVersionId, CancellationToken cancellationToken = default);
}
