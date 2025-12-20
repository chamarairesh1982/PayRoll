using Payroll.Domain.Attendance;
using Payroll.Domain.Leave;

namespace Payroll.Application.TimeReconciliation;

public class TimeReconciliationService : ITimeReconciliationService
{
    public TimeReconciliationEmployeeResult ReconcileEmployee(TimeReconciliationEmployeeInput input)
    {
        var attendanceByDay = BuildAttendanceByDay(input.Attendance, input.PeriodStart, input.PeriodEnd);
        var leaveByDay = BuildLeaveByDay(input.LeaveRequests, input.LeaveTypes, input.PeriodStart, input.PeriodEnd);

        var days = new List<DayReconciliationResult>();
        var conflicts = new List<TimeReconciliationConflict>();
        var workedDays = 0m;
        var paidLeaveDays = 0m;
        var unpaidLeaveDays = 0m;
        var absentDays = 0m;
        var noPayDays = 0m;
        var noPayHours = 0m;

        var allDates = attendanceByDay.Keys
            .Concat(leaveByDay.Keys)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        foreach (var date in allDates)
        {
            attendanceByDay.TryGetValue(date, out var attendanceEntry);
            leaveByDay.TryGetValue(date, out var leaveEntry);

            var warnings = new List<string>();
            var leaveUnits = leaveEntry?.TotalUnits ?? 0m;
            var leavePaidUnits = leaveEntry?.PaidUnits ?? 0m;
            var leaveUnpaidUnits = leaveEntry?.UnpaidUnits ?? 0m;

            if (leaveUnits > 1m)
            {
                warnings.Add("Leave units exceed a full day.");
                leaveUnits = 1m;
                leavePaidUnits = Math.Min(leavePaidUnits, 1m);
                leaveUnpaidUnits = Math.Min(leaveUnpaidUnits, 1m - leavePaidUnits);
            }

            if (leaveEntry?.HasMixedPaidStatus == true)
            {
                warnings.Add("Multiple leave types with different pay settings overlap this day.");
            }

            var hoursWorked = attendanceEntry?.HoursWorked ?? 0m;
            var hasAttendance = attendanceEntry is not null;
            var attendanceDayUnit = GetAttendanceDayUnit(hoursWorked, input.WorkingHoursPerDay, input.AttendanceHalfDayHours);
            var missingHours = Math.Max(0m, input.WorkingHoursPerDay - hoursWorked);

            decimal dayWorkedUnits = 0m;
            decimal dayAbsentUnits = 0m;
            decimal dayNoPayUnits = 0m;
            decimal dayNoPayHours = 0m;
            DayStatus status;
            string source;

            if (leaveUnits > 0m && hasAttendance && hoursWorked > 0m)
            {
                if (leaveUnits >= 1m)
                {
                    status = DayStatus.Conflict;
                    source = "Attendance+Leave";
                    warnings.Add("Attendance present with full-day leave.");
                    conflicts.Add(new TimeReconciliationConflict(
                        input.EmployeeId,
                        input.EmployeeCode,
                        input.EmployeeName,
                        date,
                        "Attendance present with full-day leave."));
                }
                else
                {
                    dayWorkedUnits = Math.Max(0m, 1m - leaveUnits);
                    dayNoPayUnits = leaveUnpaidUnits;
                    dayNoPayHours = leaveUnpaidUnits * input.WorkingHoursPerDay;
                    status = leaveUnpaidUnits > 0m ? DayStatus.LeaveUnpaidHalf : DayStatus.LeavePaidHalf;
                    source = "Attendance+Leave";
                }
            }
            else if (leaveUnits > 0m)
            {
                status = leaveUnpaidUnits > 0m
                    ? leaveUnits >= 1m ? DayStatus.LeaveUnpaidFull : DayStatus.LeaveUnpaidHalf
                    : leaveUnits >= 1m ? DayStatus.LeavePaidFull : DayStatus.LeavePaidHalf;
                source = "Leave";
                dayNoPayUnits = leaveUnpaidUnits;
                dayNoPayHours = leaveUnpaidUnits * input.WorkingHoursPerDay;
            }
            else if (hasAttendance)
            {
                if (attendanceDayUnit <= 0m)
                {
                    status = DayStatus.WorkedFull;
                    source = "Attendance";
                    dayWorkedUnits = 1m;
                }
                else
                {
                    status = attendanceDayUnit >= 1m ? DayStatus.Absent : DayStatus.WorkedPartial;
                    source = "Attendance";
                    dayNoPayUnits = attendanceDayUnit;
                    dayNoPayHours = missingHours;
                    dayAbsentUnits = attendanceDayUnit;
                    dayWorkedUnits = Math.Max(0m, 1m - attendanceDayUnit);
                }
            }
            else
            {
                continue;
            }

            workedDays += dayWorkedUnits;
            paidLeaveDays += leavePaidUnits;
            unpaidLeaveDays += leaveUnpaidUnits;
            absentDays += dayAbsentUnits;
            noPayDays += dayNoPayUnits;
            noPayHours += dayNoPayHours;

            days.Add(new DayReconciliationResult(
                date,
                status,
                source,
                dayWorkedUnits,
                leavePaidUnits,
                leaveUnpaidUnits,
                dayAbsentUnits,
                dayNoPayUnits,
                dayNoPayHours,
                warnings));
        }

        return new TimeReconciliationEmployeeResult(
            input.EmployeeId,
            input.EmployeeCode,
            input.EmployeeName,
            workedDays,
            paidLeaveDays,
            unpaidLeaveDays,
            absentDays,
            noPayDays,
            noPayHours,
            days,
            conflicts);
    }

    private static Dictionary<DateOnly, AttendanceDayEntry> BuildAttendanceByDay(
        IReadOnlyList<AttendanceRecord> attendance,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var results = new Dictionary<DateOnly, AttendanceDayEntry>();

        foreach (var record in attendance)
        {
            var overlapStart = record.Period.Start > periodStart ? record.Period.Start : periodStart;
            var overlapEnd = record.Period.End < periodEnd ? record.Period.End : periodEnd;

            if (overlapEnd < overlapStart)
            {
                continue;
            }

            foreach (var day in EnumerateDays(overlapStart, overlapEnd))
            {
                if (!results.TryGetValue(day, out var entry))
                {
                    entry = new AttendanceDayEntry();
                    results[day] = entry;
                }

                entry.HoursWorked += record.HoursWorked;
            }
        }

        return results;
    }

    private static Dictionary<DateOnly, LeaveDayEntry> BuildLeaveByDay(
        IReadOnlyList<LeaveRequest> leaveRequests,
        IReadOnlyDictionary<LeaveTypeCode, LeaveTypeDefinition> leaveTypes,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var results = new Dictionary<DateOnly, LeaveDayEntry>();

        foreach (var leave in leaveRequests)
        {
            var unitsByDay = GetOverlappingLeaveDayUnits(leave, periodStart, periodEnd);
            if (unitsByDay.Count == 0)
            {
                continue;
            }

            var typeDefinition = ResolveLeaveTypeDefinition(leave.LeaveType, leaveTypes);

            foreach (var (date, units) in unitsByDay)
            {
                if (!results.TryGetValue(date, out var entry))
                {
                    entry = new LeaveDayEntry();
                    results[date] = entry;
                }

                entry.TotalUnits += units;
                if (typeDefinition.IsPaid)
                {
                    entry.PaidUnits += units;
                }
                else
                {
                    entry.UnpaidUnits += units;
                }

                entry.HasMixedPaidStatus |= entry.PaidUnits > 0m && entry.UnpaidUnits > 0m;
            }
        }

        return results;
    }

    private static LeaveTypeDefinition ResolveLeaveTypeDefinition(
        LeaveTypeCode leaveType,
        IReadOnlyDictionary<LeaveTypeCode, LeaveTypeDefinition> leaveTypes)
    {
        if (leaveTypes.TryGetValue(leaveType, out var definition))
        {
            return definition;
        }

        return new LeaveTypeDefinition
        {
            Code = leaveType,
            Name = leaveType.ToString(),
            IsPaid = leaveType != LeaveTypeCode.NoPay,
            AllowsHalfDay = true,
            Encashable = false,
            EncashmentRateMultiplier = 1m
        };
    }

    private static decimal GetAttendanceDayUnit(decimal hoursWorked, int workingHoursPerDay, decimal attendanceHalfDayHours)
    {
        if (hoursWorked >= workingHoursPerDay)
        {
            return 0m;
        }

        if (hoursWorked <= 0m)
        {
            return 1m;
        }

        var maxHalfDayHours = Math.Min(attendanceHalfDayHours, workingHoursPerDay);
        return hoursWorked >= maxHalfDayHours ? 0.5m : 1m;
    }

    private static Dictionary<DateOnly, decimal> GetOverlappingLeaveDayUnits(
        LeaveRequest leave,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var overlapStart = leave.StartDate > periodStart ? leave.StartDate : periodStart;
        var overlapEnd = leave.EndDate < periodEnd ? leave.EndDate : periodEnd;

        if (overlapEnd < overlapStart)
        {
            return new Dictionary<DateOnly, decimal>();
        }

        var requestedUnits = leave.TotalDays > 0 ? (decimal)leave.TotalDays : overlapEnd.DayNumber - overlapStart.DayNumber + 1;

        if (leave.IsHalfDay == true)
        {
            requestedUnits = Math.Min(requestedUnits, 0.5m);
        }

        var overlapDays = overlapEnd.DayNumber - overlapStart.DayNumber + 1;
        requestedUnits = Math.Min(requestedUnits, overlapDays);

        var unitsByDay = new Dictionary<DateOnly, decimal>();
        var remaining = requestedUnits;

        for (var i = 0; i < overlapDays && remaining > 0; i++)
        {
            var day = overlapStart.AddDays(i);
            var allocation = Math.Min(1m, remaining);

            if (leave.IsHalfDay == true && requestedUnits <= 0.5m)
            {
                allocation = Math.Min(0.5m, remaining);
            }

            unitsByDay[day] = allocation;
            remaining -= allocation;
        }

        return unitsByDay;
    }

    private static IEnumerable<DateOnly> EnumerateDays(DateOnly start, DateOnly end)
    {
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private sealed class AttendanceDayEntry
    {
        public decimal HoursWorked { get; set; }
    }

    private sealed class LeaveDayEntry
    {
        public decimal TotalUnits { get; set; }
        public decimal PaidUnits { get; set; }
        public decimal UnpaidUnits { get; set; }
        public bool HasMixedPaidStatus { get; set; }
    }
}
