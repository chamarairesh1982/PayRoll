using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PaySlipDeductionLine : AuditableEntity
{
    public new Guid Id { get; set; }

    public Guid PaySlipId { get; set; }
    public PaySlip PaySlip { get; set; } = null!;

    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }

    public bool IsPreTax { get; set; }
    public bool IsPostTax { get; set; }
}
