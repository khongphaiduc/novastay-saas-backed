namespace NovaStay.Domain.ValueObject;

public readonly record struct EntityName(string Value)
{
    public override string ToString() => Value;

    public static implicit operator EntityName(string value) => new(value);

    public static implicit operator string(EntityName value) => value.Value;
}
