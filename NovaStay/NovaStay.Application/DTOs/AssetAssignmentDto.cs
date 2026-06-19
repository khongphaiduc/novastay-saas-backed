namespace NovaStay.Application.DTOs;

public sealed class AssetAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public Guid? RoomId { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public DateTime? AssignedAt { get; set; }
}
