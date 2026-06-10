namespace Host.Domain.ValueObject;

public readonly record struct EmailAddress(string Value)
{
    public override string ToString() => Value;

    public static implicit operator EmailAddress(string value) => new(value);

    public static implicit operator string(EmailAddress value) => value.Value;
}
