using Payroll.Domain.Common;
using Payroll.Domain.Payroll;

namespace Payroll.Domain.Employees;

public class EmployeePayItem : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public PayItemType PayItemType { get; set; }

    public string PayItemCode { get; set; } = null!;

    public decimal? Amount { get; set; }

    public decimal? Percentage { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }
}
