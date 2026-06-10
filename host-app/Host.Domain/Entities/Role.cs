using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Role : Entity
{
    public Guid TenantId { get; set; }
    public EntityName RoleName { get; set; }
    public Code RoleKey { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}