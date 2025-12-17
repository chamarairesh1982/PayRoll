using Payroll.Domain.Common;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Domain.Employees;

public class EmployeeRecurringPayItem : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public PayItemKind PayItemKind { get; set; }

    public Guid? AllowanceTypeId { get; set; }

    public Guid? DeductionTypeId { get; set; }

    public decimal? Amount { get; set; }

    public decimal? Percentage { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public AllowanceType? AllowanceType { get; set; }

    public DeductionType? DeductionType { get; set; }
}
