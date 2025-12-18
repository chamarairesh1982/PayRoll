using Payroll.Domain.Common;

namespace Payroll.Domain.Organizations;

public class Branch : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public ICollection<CostCenter> CostCenters { get; private set; } = new List<CostCenter>();

    private Branch()
    {
    }

    public Branch(string code, string name, Guid companyId)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
        CompanyId = companyId;
    }

    public void Update(string code, string name, Guid companyId)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
        CompanyId = companyId;
    }

    private static string ValidateRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} is required", propertyName);
        }

        return value.Trim();
    }
}
