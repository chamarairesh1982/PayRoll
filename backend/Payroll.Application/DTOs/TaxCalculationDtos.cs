namespace Payroll.Application.DTOs;

public record TaxCalculationBreakdownLine(
    decimal BandFrom,
    decimal? BandTo,
    decimal Rate,
    decimal TaxableInBand,
    decimal TaxForBand);

public record TaxCalculationSummaryDto(
    Guid? SlabSetId,
    decimal TaxableEarnings,
    decimal ReliefTotal,
    decimal TaxableBase,
    decimal Tax,
    IReadOnlyList<TaxCalculationBreakdownLine> Breakdown);

public record TaxPreviewRequest(Guid EmployeeId, DateTime PeriodStart, DateTime PeriodEnd);
