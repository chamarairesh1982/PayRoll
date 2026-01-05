using Payroll.Domain.Attendance;
using Payroll.Domain.Leave;

namespace Payroll.Application.TimeReconciliation;

public enum DayStatus
{
    WorkedFull,
    WorkedPartial,
    Absent,
    LeavePaidFull,
    LeavePaidHalf,
    LeaveUnpaidFull,
    LeaveUnpaidHalf,
    Conflict
}

public record DayReconciliationResult(
    DateOnly Date,
    DayStatus Status,
    string Source,
    decimal WorkedUnits,
    decimal PaidLeaveUnits,
    decimal UnpaidLeaveUnits,
    decimal AbsentUnits,
    decimal NoPayDayUnits,
    decimal NoPayHours,
    IReadOnlyList<string> Warnings);

public record TimeReconciliationConflict(
    Guid EmployeeId,
    string? EmployeeCode,
    string? EmployeeName,
    DateOnly Date,
    string Warning);

public record TimeReconciliationEmployeeResult(
    Guid EmployeeId,
    string? EmployeeCode,
    string? EmployeeName,
    decimal WorkedDays,
    decimal PaidLeaveDays,
    decimal UnpaidLeaveDays,
    decimal AbsentDays,
    decimal NoPayDays,
    decimal NoPayHours,
    IReadOnlyList<DayReconciliationResult> Days,
    IReadOnlyList<TimeReconciliationConflict> Conflicts);

public record TimeReconciliationEmployeeInput(
    Guid EmployeeId,
    string? EmployeeCode,
    string? EmployeeName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int WorkingHoursPerDay,
    decimal AttendanceHalfDayHours,
    IReadOnlyList<AttendanceRecord> Attendance,
    IReadOnlyList<LeaveRequest> LeaveRequests,
    IReadOnlyDictionary<LeaveTypeCode, LeaveTypeDefinition> LeaveTypes);
