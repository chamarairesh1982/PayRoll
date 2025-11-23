namespace Payroll.Domain.ValueObjects;

public readonly record struct DateRange(DateOnly Start, DateOnly End)
{
    public bool Contains(DateOnly date) => date >= Start && date <= End;
}
