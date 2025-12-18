using Payroll.Domain.GeneralLedger;
using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class GeneralLedgerAccountMappingDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GeneralLedgerMappingType MappingType { get; set; }
    public string DebitAccount { get; set; } = string.Empty;
    public string CreditAccount { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpsertGeneralLedgerAccountMappingRequest
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GeneralLedgerMappingType MappingType { get; set; }
    public string DebitAccount { get; set; } = string.Empty;
    public string CreditAccount { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class GeneralLedgerJournalEntryDto
{
    public string DebitAccount { get; set; } = string.Empty;
    public string CreditAccount { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Narrative { get; set; } = string.Empty;
}

public class GeneralLedgerExportDto
{
    public Guid PayRunId { get; set; }
    public GeneralLedgerExportStatus Status { get; set; }
    public List<GeneralLedgerJournalEntryDto> Entries { get; set; } = new();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class GeneralLedgerActionRequest
{
    public string? Comment { get; set; }
}
