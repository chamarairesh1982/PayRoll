namespace Payroll.Domain.PayrollConfig;

public enum CalculationBasis
{
    FixedAmount = 1,
    PercentageOfBasic = 2,
    PercentageOfGross = 3,
    PerDay = 4,
    PerHour = 5
}
