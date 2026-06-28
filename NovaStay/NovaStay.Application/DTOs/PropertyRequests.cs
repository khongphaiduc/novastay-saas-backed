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

public sealed class CreatePropertyServiceRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public string? ServiceCode { get; set; }
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public string? Unit { get; set; }
    public string? BillingCycle { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdatePropertyServiceRequest
{
    public decimal DefaultPrice { get; set; }
    public string? BillingCycle { get; set; }
    public bool IsActive { get; set; }
}
