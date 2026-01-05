using Payroll.Application.DTOs.RulePackages;

namespace Payroll.Application.Interfaces;

public interface IRulePackageService
{
    Task<IReadOnlyList<RulePackageDto>> GetPackagesAsync(RulePackageQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RulePackageVersionDto>> GetVersionsAsync(Guid packageId, CancellationToken cancellationToken = default);
    Task<RulePackageDto> CreatePackageAsync(CreateRulePackageRequest request, CancellationToken cancellationToken = default);
    Task<RulePackageVersionDto> CreateVersionAsync(Guid packageId, CreateRulePackageVersionRequest request, CancellationToken cancellationToken = default);
    Task ActivateVersionAsync(Guid packageId, Guid versionId, CancellationToken cancellationToken = default);
}
