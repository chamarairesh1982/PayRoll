using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class PayRunSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PayPeriodType PeriodType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public bool IsConsolidated { get; set; }
    public PayRunStatus Status { get; set; }
    public bool IsLocked { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalNetPay { get; set; }
}

public class PayRunDetailDto : PayRunSummaryDto
{
    public List<PaySlipDto> PaySlips { get; set; } = new();
    public List<PayRunApprovalDto> Approvals { get; set; } = new();
}

public class PayRunApprovalDto
{
    public Guid Id { get; set; }
    public PayRunStatus FromStatus { get; set; }
    public PayRunStatus ToStatus { get; set; }
    public string ActionedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionedAt { get; set; }
}
