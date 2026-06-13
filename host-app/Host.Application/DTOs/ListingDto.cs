namespace Host.Application.DTOs;

public sealed class ListingDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Amenities { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
}
