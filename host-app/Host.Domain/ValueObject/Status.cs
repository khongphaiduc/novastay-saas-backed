namespace Host.Domain.ValueObject;

public readonly record struct Status(string Value)
{
    public override string ToString() => Value;

    public static implicit operator Status(string value) => new(value);

    public static implicit operator string(Status value) => value.Value;
}
