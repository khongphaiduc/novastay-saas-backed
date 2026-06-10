using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Listing : Entity
{
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? BedId { get; set; }
    public EntityName Title { get; set; }
    public string? Description { get; set; }
    public string? Amenities { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
}