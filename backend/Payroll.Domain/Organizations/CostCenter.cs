using Payroll.Domain.Common;

namespace Payroll.Domain.Organizations;

public class CostCenter : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? BranchId { get; private set; }
    public Branch? Branch { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Company? Company { get; private set; }

    private CostCenter()
    {
    }

    public CostCenter(string code, string name, Guid? branchId, Guid? companyId)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
        SetScope(branchId, companyId);
    }

    public void Update(string code, string name, Guid? branchId, Guid? companyId)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
        SetScope(branchId, companyId);
    }

    private static string ValidateRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} is required", propertyName);
        }

        return value.Trim();
    }

    private void SetScope(Guid? branchId, Guid? companyId)
    {
        if (!branchId.HasValue && !companyId.HasValue)
        {
            throw new ArgumentException("A cost center must belong to a branch or a company.");
        }

        if (branchId.HasValue && companyId.HasValue)
        {
            throw new ArgumentException("A cost center cannot belong to both a branch and a company.");
        }

        BranchId = branchId;
        CompanyId = companyId;
    }
}
