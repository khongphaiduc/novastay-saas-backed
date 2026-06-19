namespace NovaStay.Domain.ValueObject;

public readonly record struct Code(string Value)
{
    public override string ToString() => Value;

    public static implicit operator Code(string value) => new(value);

    public static implicit operator string(Code value) => value.Value;
}
