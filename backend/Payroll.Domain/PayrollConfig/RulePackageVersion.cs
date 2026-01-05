using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class RulePackageVersion : AuditableEntity
{
    public Guid RulePackageId { get; set; }
    public RulePackage RulePackage { get; set; } = null!;
    public int VersionNumber { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public RulePackageVersionStatus Status { get; set; } = RulePackageVersionStatus.Draft;
    public string ContentJson { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
}

public enum RulePackageVersionStatus
{
    Draft = 1,
    Active = 2,
    Retired = 3
}
