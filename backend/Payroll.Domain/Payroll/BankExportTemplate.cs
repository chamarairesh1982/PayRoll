using Payroll.Domain.Common;

namespace Payroll.Domain.Payroll;

public class BankExportTemplate : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public BankExportFormat Format { get; set; } = BankExportFormat.Csv;
    public string? Delimiter { get; set; }
    public int HeaderRowCount { get; set; }
}

public enum BankExportFormat
{
    Csv = 1,
    Txt = 2,
    FixedWidth = 3
}
