using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class RulePackage : AuditableEntity, IAggregateRoot
{
    public Guid CompanyId { get; set; }
    public RulePackageType RuleType { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RulePackageVersion> Versions { get; set; } = new List<RulePackageVersion>();
}

public enum RulePackageType
{
    Tax = 1,
    Epf = 2,
    Etf = 3,
    Other = 4
}
