using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class CreatePayRunRequest
{
    public string Name { get; set; } = string.Empty;
    public PayPeriodType PeriodType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public bool IsConsolidated { get; set; }
    public bool IncludeActiveEmployeesOnly { get; set; } = true;
    public List<Guid>? EmployeeIds { get; set; }
}

public class RecalculatePayRunRequest
{
    public bool IncludeAttendance { get; set; } = true;
    public bool IncludeOvertime { get; set; } = true;
    public bool IncludeLoans { get; set; } = true;
    public bool IncludeAllowancesAndDeductions { get; set; } = true;
}

public class ChangePayRunStatusRequest
{
    public PayRunStatus Status { get; set; }
}

public class PayRunActionRequest
{
    public string ActionedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

public class PayRunQuery
{
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public bool? IsConsolidated { get; set; }
    public PayRunStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
