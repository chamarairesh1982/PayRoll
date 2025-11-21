using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PaySlipEarningLine : AuditableEntity
{
    public new Guid Id { get; set; }

    public Guid PaySlipId { get; set; }
    public PaySlip PaySlip { get; set; } = null!;

    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }

    public bool IsEpfApplicable { get; set; }
    public bool IsEtfApplicable { get; set; }
    public bool IsTaxable { get; set; }
}
