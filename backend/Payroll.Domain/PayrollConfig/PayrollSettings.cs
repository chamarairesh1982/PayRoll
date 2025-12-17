using Payroll.Domain.Common;

namespace Payroll.Domain.PayrollConfig;

public class PayrollSettings : AuditableEntity
{
    public int WorkingDaysPerMonth { get; set; }

    public int WorkingHoursPerDay { get; set; }

    public decimal WeekdayOvertimeMultiplier { get; set; }

    public decimal WeekendOvertimeMultiplier { get; set; }

    public decimal HolidayOvertimeMultiplier { get; set; }
}
