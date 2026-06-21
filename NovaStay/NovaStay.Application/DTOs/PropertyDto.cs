namespace NovaStay.Application.DTOs;

public sealed class PropertyDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}