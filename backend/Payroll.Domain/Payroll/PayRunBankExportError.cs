using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class PayRunBankExportError : AuditableEntity
{
    public Guid PayRunBankExportId { get; set; }
    public PayRunBankExport? PayRunBankExport { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
