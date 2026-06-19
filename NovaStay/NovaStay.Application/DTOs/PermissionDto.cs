namespace NovaStay.Application.DTOs;

public sealed class PermissionDto
{
    public Guid Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}