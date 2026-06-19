using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class PermissionEntity : Entity
{
    public EntityName PermissionName { get; set; }
    public Code PermissionKey { get; set; }
    public Code Module { get; set; }
}