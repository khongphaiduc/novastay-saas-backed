namespace Host.Domain.ValueObject;

public readonly record struct UrlValue(string Value)
{
    public override string ToString() => Value;

    public static implicit operator UrlValue(string value) => new(value);

    public static implicit operator string(UrlValue value) => value.Value;
}
