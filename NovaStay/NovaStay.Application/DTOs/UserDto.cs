namespace NovaStay.Application.DTOs;

public sealed class UserDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}