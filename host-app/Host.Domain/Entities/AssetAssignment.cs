using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class AssetAssignment : Entity
{
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? BedId { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public DateTime? AssignedAt { get; set; }
}