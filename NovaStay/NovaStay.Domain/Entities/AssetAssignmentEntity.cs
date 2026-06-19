using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class AssetAssignmentEntity : Entity
{
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public Guid? RoomId { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public DateTime? AssignedAt { get; set; }
}
