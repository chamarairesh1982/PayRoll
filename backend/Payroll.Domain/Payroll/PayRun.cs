using Payroll.Domain.Common;
using Payroll.Domain.Employees;
using Payroll.Domain.Organizations;

namespace Payroll.Domain.Payroll;

public class PayRun : AuditableEntity, IAggregateRoot
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PayPeriodType PeriodType { get; set; } = PayPeriodType.Monthly;
    public string Reference { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public bool IsLocked { get; set; }
    public bool IsConsolidated { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public PayRunStatus Status { get; set; } = PayRunStatus.Draft;
    public BankExportStatus ExportStatus { get; set; } = BankExportStatus.Pending;
    public string? ExportedBank { get; set; }
    public DateTime? ExportedAt { get; set; }
    public DateTime? ExportDownloadedAt { get; set; }
    public GeneralLedgerExportStatus GeneralLedgerStatus { get; set; } = GeneralLedgerExportStatus.Pending;
    public DateTime? GeneralLedgerReviewedAt { get; set; }
    public string? GeneralLedgerReviewedByUserId { get; set; }
    public string? GeneralLedgerReviewedByUserName { get; set; }
    public DateTime? GeneralLedgerApprovedAt { get; set; }
    public string? GeneralLedgerApprovedByUserId { get; set; }
    public string? GeneralLedgerApprovedByUserName { get; set; }
    public DateTime? GeneralLedgerExportedAt { get; set; }
    public DateTime? PreparedAt { get; set; }
    public string? PreparedByUserId { get; set; }
    public string? PreparedByUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? LockedByUserId { get; set; }
    public string? LockedByUserName { get; set; }
    public ICollection<PaySlip> PaySlips { get; set; } = new List<PaySlip>();
    public ICollection<PayRunStatusHistory> StatusHistory { get; set; } = new List<PayRunStatusHistory>();
    public ICollection<PayRunBankExport> BankExports { get; set; } = new List<PayRunBankExport>();
}

public enum PayPeriodType
{
    Monthly = 1,
    Weekly = 2,
    Custom = 3
}
