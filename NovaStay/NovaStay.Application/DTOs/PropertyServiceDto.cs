namespace NovaStay.Application.DTOs;

public sealed class PropertyServiceDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? ServiceCode { get; set; }
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public string? Unit { get; set; }
    public string? BillingCycle { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
