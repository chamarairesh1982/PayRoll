namespace Payroll.Application.DTOs;

public class TimeReconciliationResultDto
{
    public Guid PayRunId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<TimeReconciliationEmployeeSummaryDto> Employees { get; set; } = new();
    public List<TimeReconciliationConflictDto> Conflicts { get; set; } = new();
}

public class TimeReconciliationEmployeeSummaryDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal NoPayDays { get; set; }
    public decimal NoPayHours { get; set; }
}

public class TimeReconciliationConflictDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public string Warning { get; set; } = string.Empty;
}
