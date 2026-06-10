using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class MaintenanceTicket : Entity
{
    public Guid TenantId { get; set; }
    public Guid RoomId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid? AssetAssignmentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string UserDescription { get; set; } = string.Empty;
    public string? IncidentImageUrl { get; set; }
    public string? Status { get; set; }
    public Guid? TechnicianId { get; set; }
    public string? ResolvedImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}