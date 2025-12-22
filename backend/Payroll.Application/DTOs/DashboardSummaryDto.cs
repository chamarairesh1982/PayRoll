namespace Payroll.Application.DTOs;

public class DashboardSummaryDto
{
    public DashboardKpiDto Kpis { get; set; } = new();
    public DashboardHealthDto Health { get; set; } = new();
    public List<DashboardActivityDto> RecentActivity { get; set; } = new();
}

public class DashboardKpiDto
{
    public DateTime? NextPayDate { get; set; }
    public int EmployeeCount { get; set; }
    public string LatestPayRunStatus { get; set; } = string.Empty;
    public int ExceptionsCount { get; set; }
}

public class DashboardHealthDto
{
    public int MissingEpf { get; set; }
    public int MissingBank { get; set; }
    public int PendingAttendance { get; set; }
    public int PendingOt { get; set; }
    public int NegativeNet { get; set; }
}

public class DashboardActivityDto
{
    public DateTime OccurredAt { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public record DashboardSummaryRequest(
    DateTime? PeriodStart,
    DateTime? PeriodEnd,
    Guid? CompanyId,
    Guid? BranchId,
    Guid? CostCenterId);
