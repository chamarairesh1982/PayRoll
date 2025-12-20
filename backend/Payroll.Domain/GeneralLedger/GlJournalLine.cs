using Payroll.Domain.Common;
using Payroll.Domain.Employees;
using Payroll.Domain.Organizations;

namespace Payroll.Domain.GeneralLedger;

public class GlJournalLine : AuditableEntity
{
    public Guid BatchId { get; set; }
    public GlJournalBatch? Batch { get; set; }
    public DateTime PostingDate { get; set; }
    public Guid AccountId { get; set; }
    public GlAccount? Account { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string? PayComponentCode { get; set; }
    public GlPayComponentType? PayComponentType { get; set; }
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Reference { get; set; } = string.Empty;
}
