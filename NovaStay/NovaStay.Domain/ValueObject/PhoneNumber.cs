namespace NovaStay.Domain.ValueObject;

public readonly record struct PhoneNumber(string Value)
{
    public override string ToString() => Value;

    public static implicit operator PhoneNumber(string value) => new(value);

    public static implicit operator string(PhoneNumber value) => value.Value;
}
