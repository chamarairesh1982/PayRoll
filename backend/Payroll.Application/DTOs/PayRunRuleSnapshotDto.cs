namespace Payroll.Application.DTOs;

public class PayRunRuleSnapshotDto
{
    public Guid PayRunId { get; set; }
    public Guid? TaxRuleVersionId { get; set; }
    public Guid? EpfRuleVersionId { get; set; }
    public Guid? EtfRuleVersionId { get; set; }
    public string? RulesSnapshotJson { get; set; }
}
