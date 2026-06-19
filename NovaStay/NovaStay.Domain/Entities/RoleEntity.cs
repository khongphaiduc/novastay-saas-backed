using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class RoleEntity : Entity
{
    public Guid TenantId { get; set; }
    public EntityName RoleName { get; set; }
    public Code RoleKey { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}