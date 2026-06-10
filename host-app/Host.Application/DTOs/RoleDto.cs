namespace Host.Application.DTOs;

public sealed class RoleDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}