using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.DTOs.RulePackages;

public class RuleVersionSnapshotDto
{
    public RuleVersionSnapshotItemDto? Tax { get; init; }
    public RuleVersionSnapshotItemDto? Epf { get; init; }
    public RuleVersionSnapshotItemDto? Etf { get; init; }
}

public class RuleVersionSnapshotItemDto
{
    public Guid PackageId { get; init; }
    public Guid VersionId { get; init; }
    public int VersionNumber { get; init; }
    public RulePackageType RuleType { get; init; }
    public Guid CompanyId { get; init; }
    public string PackageName { get; init; } = string.Empty;
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public RulePackageVersionStatus Status { get; init; }
    public string ContentHash { get; init; } = string.Empty;
}
