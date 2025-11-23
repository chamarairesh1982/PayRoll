namespace Payroll.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
