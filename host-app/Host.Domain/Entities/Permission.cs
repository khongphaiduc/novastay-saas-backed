using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Permission : Entity
{
    public EntityName PermissionName { get; set; }
    public Code PermissionKey { get; set; }
    public Code Module { get; set; }
}