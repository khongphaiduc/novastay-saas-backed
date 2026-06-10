using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class User : Entity
{
    public Guid TenantId { get; set; }
    public EntityName FullName { get; set; }
    public string? Email { get; set; }
    public PhoneNumber Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}