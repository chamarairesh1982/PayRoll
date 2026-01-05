using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs.RulePackages;
using Payroll.Application.Interfaces;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.RulePackages;

public class RuleVersionResolver : IRuleVersionResolver
{
    private static readonly JsonSerializerOptions SnapshotSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IPayrollDbContext _dbContext;

    public RuleVersionResolver(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RuleVersionResolutionResult> ResolveRuleVersionsAsync(Guid? companyId, DateOnly periodEndDate, CancellationToken cancellationToken = default)
    {
        var resolvedTax = await GetActiveVersionAsync(RulePackageType.Tax, companyId, periodEndDate, cancellationToken);
        var resolvedEpf = await GetActiveVersionAsync(RulePackageType.Epf, companyId, periodEndDate, cancellationToken);
        var resolvedEtf = await GetActiveVersionAsync(RulePackageType.Etf, companyId, periodEndDate, cancellationToken);

        return new RuleVersionResolutionResult(resolvedTax, resolvedEpf, resolvedEtf, BuildSnapshotJson(resolvedTax, resolvedEpf, resolvedEtf));
    }

    public async Task<RuleVersionResolutionResult> ResolveRuleVersionsByIdsAsync(Guid? taxVersionId, Guid? epfVersionId, Guid? etfVersionId, CancellationToken cancellationToken = default)
    {
        var taxVersion = await GetVersionByIdAsync(taxVersionId, cancellationToken);
        var epfVersion = await GetVersionByIdAsync(epfVersionId, cancellationToken);
        var etfVersion = await GetVersionByIdAsync(etfVersionId, cancellationToken);

        return new RuleVersionResolutionResult(taxVersion, epfVersion, etfVersion, BuildSnapshotJson(taxVersion, epfVersion, etfVersion));
    }

    private async Task<RulePackageVersion?> GetActiveVersionAsync(
        RulePackageType ruleType,
        Guid? companyId,
        DateOnly periodEndDate,
        CancellationToken cancellationToken)
    {
        var resolvedCompanyId = companyId ?? Guid.Empty;

        return await _dbContext.RulePackageVersions
            .Include(v => v.RulePackage)
            .Where(v => v.RulePackage.RuleType == ruleType
                        && v.RulePackage.CompanyId == resolvedCompanyId
                        && v.Status == RulePackageVersionStatus.Active
                        && v.EffectiveFrom <= periodEndDate
                        && (v.EffectiveTo == null || v.EffectiveTo >= periodEndDate))
            .OrderByDescending(v => v.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<RulePackageVersion?> GetVersionByIdAsync(Guid? versionId, CancellationToken cancellationToken)
    {
        if (!versionId.HasValue)
        {
            return null;
        }

        return await _dbContext.RulePackageVersions
            .Include(v => v.RulePackage)
            .FirstOrDefaultAsync(v => v.Id == versionId.Value, cancellationToken);
    }

    private static string BuildSnapshotJson(RulePackageVersion? tax, RulePackageVersion? epf, RulePackageVersion? etf)
    {
        var snapshot = new RuleVersionSnapshotDto
        {
            Tax = BuildSnapshotItem(tax),
            Epf = BuildSnapshotItem(epf),
            Etf = BuildSnapshotItem(etf)
        };

        return JsonSerializer.Serialize(snapshot, SnapshotSerializerOptions);
    }

    private static RuleVersionSnapshotItemDto? BuildSnapshotItem(RulePackageVersion? version)
    {
        if (version?.RulePackage == null)
        {
            return null;
        }

        return new RuleVersionSnapshotItemDto
        {
            PackageId = version.RulePackageId,
            VersionId = version.Id,
            VersionNumber = version.VersionNumber,
            RuleType = version.RulePackage.RuleType,
            CompanyId = version.RulePackage.CompanyId,
            PackageName = version.RulePackage.Name,
            EffectiveFrom = version.EffectiveFrom,
            EffectiveTo = version.EffectiveTo,
            Status = version.Status,
            ContentHash = version.ContentHash
        };
    }
}
