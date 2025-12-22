using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.DTOs.RulePackages;

public class RulePackageDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public RulePackageType RuleType { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RulePackageVersionDto
{
    public Guid Id { get; set; }
    public Guid RulePackageId { get; set; }
    public int VersionNumber { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public RulePackageVersionStatus Status { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RulePackageQuery
{
    public RulePackageType? Type { get; set; }
    public Guid? CompanyId { get; set; }
}

public class CreateRulePackageRequest
{
    public Guid CompanyId { get; set; }
    public RulePackageType RuleType { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateRulePackageVersionRequest
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ContentJson { get; set; } = string.Empty;
}
