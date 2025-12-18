using Payroll.Domain.Common;

namespace Payroll.Domain.Organizations;

public class CostCenter : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid BranchId { get; private set; }
    public Branch? Branch { get; private set; }

    private CostCenter()
    {
    }

    public CostCenter(string code, string name, Guid branchId)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
        BranchId = branchId;
    }

    public void Update(string code, string name, Guid branchId)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
        BranchId = branchId;
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
