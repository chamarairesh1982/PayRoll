using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs.RulePackages;
using Payroll.Application.Interfaces;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.RulePackages;

public class RulePackageService : IRulePackageService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RulePackageService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RulePackageDto>> GetPackagesAsync(RulePackageQuery query, CancellationToken cancellationToken = default)
    {
        var packagesQuery = _dbContext.RulePackages.AsNoTracking();

        if (query.Type.HasValue)
        {
            packagesQuery = packagesQuery.Where(p => p.RuleType == query.Type.Value);
        }

        if (query.CompanyId.HasValue)
        {
            packagesQuery = packagesQuery.Where(p => p.CompanyId == query.CompanyId.Value);
        }

        var packages = await packagesQuery
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return packages.Select(MapPackage).ToList();
    }

    public async Task<IReadOnlyList<RulePackageVersionDto>> GetVersionsAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        var versions = await _dbContext.RulePackageVersions
            .AsNoTracking()
            .Where(v => v.RulePackageId == packageId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

        return versions.Select(MapVersion).ToList();
    }

    public async Task<RulePackageDto> CreatePackageAsync(CreateRulePackageRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.RulePackages
            .AnyAsync(p => p.CompanyId == request.CompanyId
                           && p.RuleType == request.RuleType
                           && p.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Rule package already exists for the company and type.");
        }

        var package = new RulePackage
        {
            CompanyId = request.CompanyId,
            RuleType = request.RuleType,
            Name = request.Name.Trim(),
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.RulePackages.AddAsync(package, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapPackage(package);
    }

    public async Task<RulePackageVersionDto> CreateVersionAsync(Guid packageId, CreateRulePackageVersionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
        {
            throw new InvalidOperationException("Effective to date cannot be earlier than effective from date.");
        }

        var package = await _dbContext.RulePackages
            .FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);

        if (package is null)
        {
            throw new KeyNotFoundException("Rule package not found.");
        }

        var nextVersion = await _dbContext.RulePackageVersions
            .Where(v => v.RulePackageId == packageId)
            .Select(v => v.VersionNumber)
            .DefaultIfEmpty(0)
            .MaxAsync(cancellationToken) + 1;

        var normalizedJson = request.ContentJson.Trim();
        if (string.IsNullOrWhiteSpace(normalizedJson))
        {
            throw new InvalidOperationException("Content JSON cannot be empty.");
        }

        var version = new RulePackageVersion
        {
            RulePackageId = packageId,
            VersionNumber = nextVersion,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Status = RulePackageVersionStatus.Draft,
            ContentJson = normalizedJson,
            ContentHash = ComputeHash(normalizedJson),
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.RulePackageVersions.AddAsync(version, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapVersion(version);
    }

    public async Task ActivateVersionAsync(Guid packageId, Guid versionId, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.RulePackageVersions
            .Include(v => v.RulePackage)
            .FirstOrDefaultAsync(v => v.Id == versionId && v.RulePackageId == packageId, cancellationToken);

        if (version is null)
        {
            throw new KeyNotFoundException("Rule package version not found.");
        }

        var rangeStart = version.EffectiveFrom;
        var rangeEnd = version.EffectiveTo ?? DateOnly.MaxValue;

        var overlaps = await _dbContext.RulePackageVersions
            .AnyAsync(existing => existing.RulePackageId == packageId
                                  && existing.Id != version.Id
                                  && existing.Status == RulePackageVersionStatus.Active
                                  && existing.EffectiveFrom <= rangeEnd
                                  && (existing.EffectiveTo == null || existing.EffectiveTo >= rangeStart),
                cancellationToken);

        if (overlaps)
        {
            throw new InvalidOperationException("Overlapping active versions are not allowed.");
        }

        var activeVersions = await _dbContext.RulePackageVersions
            .Where(v => v.RulePackageId == packageId && v.Status == RulePackageVersionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var active in activeVersions)
        {
            active.Status = RulePackageVersionStatus.Retired;
            active.ModifiedAt = DateTime.UtcNow;
            active.ModifiedBy = _currentUserService.UserName ?? "system";
        }

        version.Status = RulePackageVersionStatus.Active;
        version.ModifiedAt = DateTime.UtcNow;
        version.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static RulePackageDto MapPackage(RulePackage package)
    {
        return new RulePackageDto
        {
            Id = package.Id,
            CompanyId = package.CompanyId,
            RuleType = package.RuleType,
            Name = package.Name,
            CreatedAt = package.CreatedAt
        };
    }

    private static RulePackageVersionDto MapVersion(RulePackageVersion version)
    {
        return new RulePackageVersionDto
        {
            Id = version.Id,
            RulePackageId = version.RulePackageId,
            VersionNumber = version.VersionNumber,
            EffectiveFrom = version.EffectiveFrom,
            EffectiveTo = version.EffectiveTo,
            Status = version.Status,
            ContentJson = version.ContentJson,
            ContentHash = version.ContentHash,
            CreatedAt = version.CreatedAt
        };
    }

    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}
