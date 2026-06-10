namespace Host.Domain.ValueObject;

public readonly record struct Money(decimal Amount)
{
    public static Money Zero => new(0m);

    public static implicit operator Money(decimal amount) => new(amount);

    public static implicit operator decimal(Money money) => money.Amount;
}
