namespace NovaStay.Application.DTOs;

public sealed class CreatePropertyRequest
{
    public string PropertyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PropertyType { get; set; } = "BoardingHouse";
}

public sealed class UpdatePropertyRequest
{
    public string PropertyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
}
