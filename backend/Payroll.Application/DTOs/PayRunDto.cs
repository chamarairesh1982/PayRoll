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
    public BankExportStatus ExportStatus { get; set; }
    public string? ExportedBank { get; set; }
    public DateTime? ExportedAt { get; set; }
    public DateTime? ExportDownloadedAt { get; set; }
    public GeneralLedgerExportStatus GeneralLedgerStatus { get; set; }
    public DateTime? GeneralLedgerReviewedAt { get; set; }
    public string? GeneralLedgerReviewedByUserName { get; set; }
    public DateTime? GeneralLedgerApprovedAt { get; set; }
    public string? GeneralLedgerApprovedByUserName { get; set; }
    public DateTime? GeneralLedgerExportedAt { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalNetPay { get; set; }
}

public class PayRunDetailDto : PayRunSummaryDto
{
    public List<PaySlipDto> PaySlips { get; set; } = new();
    public List<PayRunStatusHistoryDto> StatusHistory { get; set; } = new();
    public DateTime? PreparedAt { get; set; }
    public string? PreparedByUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? LockedByUserName { get; set; }
}

public class PayRunStatusHistoryDto
{
    public Guid Id { get; set; }
    public PayRunStatus FromStatus { get; set; }
    public PayRunStatus ToStatus { get; set; }
    public string ActorUserName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? ActorUserId { get; set; }
    public DateTime ActionedAt { get; set; }
}
