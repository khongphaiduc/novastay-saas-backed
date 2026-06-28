namespace NovaStay.Application.DTOs;

// TASK-021: Xem lịch sử bảo trì phòng / Request tạo ticket
public sealed class CreateMaintenanceTicketRequest
{
    public Guid OrganizationId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? ResidentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string UserDescription { get; set; } = string.Empty;
}

// Enriched maintenance ticket DTO
public sealed class MaintenanceTicketDetailDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? ResidentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string UserDescription { get; set; } = string.Empty;
    public string? IncidentImageUrl { get; set; }
    public string? Status { get; set; }
    public Guid? TechnicianId { get; set; }
    public string? ResolvedImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // Enriched
    public string? ResidentName { get; set; }
    public string? RoomNumber { get; set; }
}
