using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.DTOs.RulePackages;

public record RuleVersionResolutionResult(
    RulePackageVersion? TaxVersion,
    RulePackageVersion? EpfVersion,
    RulePackageVersion? EtfVersion,
    string SnapshotJson);
