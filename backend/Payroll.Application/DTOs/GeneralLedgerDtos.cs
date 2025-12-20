using Payroll.Domain.GeneralLedger;

namespace Payroll.Application.DTOs;

public class GlAccountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GlAccountType Type { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertGlAccountRequest
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GlAccountType Type { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GlMappingDto
{
    public Guid? Id { get; set; }
    public string PayComponentCode { get; set; } = string.Empty;
    public string PayComponentName { get; set; } = string.Empty;
    public GlPayComponentType PayComponentType { get; set; }
    public Guid? DebitAccountId { get; set; }
    public string? DebitAccountCode { get; set; }
    public string? DebitAccountName { get; set; }
    public Guid? CreditAccountId { get; set; }
    public string? CreditAccountCode { get; set; }
    public string? CreditAccountName { get; set; }
    public GlPostingSideRule PostingSideRule { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public string? Notes { get; set; }
}

public class UpsertGlMappingRequest
{
    public Guid? Id { get; set; }
    public string PayComponentCode { get; set; } = string.Empty;
    public GlPayComponentType PayComponentType { get; set; }
    public Guid? DebitAccountId { get; set; }
    public Guid? CreditAccountId { get; set; }
    public GlPostingSideRule PostingSideRule { get; set; } = GlPostingSideRule.DebitWhenPositive;
    public Guid? CostCenterId { get; set; }
    public string? Notes { get; set; }
}

public class GlJournalLineDto
{
    public Guid Id { get; set; }
    public DateTime PostingDate { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? PayComponentCode { get; set; }
    public GlPayComponentType? PayComponentType { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string Reference { get; set; } = string.Empty;
}

public class GlJournalBatchSummaryDto
{
    public Guid Id { get; set; }
    public Guid PayRunId { get; set; }
    public GlJournalBatchStatus Status { get; set; }
    public DateTime? GeneratedAtUtc { get; set; }
    public string? GeneratedByUserName { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ExportedAtUtc { get; set; }
    public string? ExportedByUserName { get; set; }
    public string? Notes { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced { get; set; }
}

public class GlJournalBatchDetailDto : GlJournalBatchSummaryDto
{
    public List<GlJournalLineDto> Lines { get; set; } = new();
}

public class GlJournalBatchActionRequest
{
    public string? Comment { get; set; }
}
