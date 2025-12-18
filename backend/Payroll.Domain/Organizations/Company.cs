using Payroll.Domain.Common;

namespace Payroll.Domain.Organizations;

public class Company : AuditableEntity, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ICollection<Branch> Branches { get; private set; } = new List<Branch>();

    private Company()
    {
    }

    public Company(string code, string name)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
    }

    public void Update(string code, string name)
    {
        Code = ValidateRequired(code, nameof(Code));
        Name = ValidateRequired(name, nameof(Name));
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
