namespace Payroll.Domain.ValueObjects;

public sealed record DateRange
{
    public DateOnly Start { get; init; }

    public DateOnly End { get; init; }

    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentException("End date must be on or after start date", nameof(end));
        }

        Start = start;
        End = end;
    }

    private DateRange()
    {
        Start = default;
        End = default;
    }

    public bool Contains(DateOnly date) => date >= Start && date <= End;
}
