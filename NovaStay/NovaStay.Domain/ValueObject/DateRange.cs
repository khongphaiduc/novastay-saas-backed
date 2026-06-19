namespace NovaStay.Domain.ValueObject;

public readonly record struct DateRange(DateOnly StartDate, DateOnly? EndDate)
{
    public bool HasEndDate => EndDate.HasValue;
}
