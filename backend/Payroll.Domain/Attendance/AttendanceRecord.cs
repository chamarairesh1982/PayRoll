using Payroll.Domain.Common;
using Payroll.Domain.ValueObjects;

namespace Payroll.Domain.Attendance;

public class AttendanceRecord : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public DateRange Period { get; set; }
    public decimal HoursWorked { get; set; }
}
