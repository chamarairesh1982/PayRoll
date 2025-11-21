namespace Payroll.Domain.ValueObjects;

public readonly record struct DateRange(DateTime Start, DateTime End)
{
    public bool Contains(DateTime date) => date >= Start && date <= End;
}
