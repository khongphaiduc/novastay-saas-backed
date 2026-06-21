namespace NovaStay.Application.DTOs;

public sealed class MaintenanceTicketDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
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