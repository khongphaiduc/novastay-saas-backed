using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class PropertyEntity : Entity
{
    public Guid TenantId { get; set; }
    public EntityName PropertyName { get; set; }
    public string Address { get; set; } = string.Empty;
    public Code PropertyType { get; set; }
    public DateTime? CreatedAt { get; set; }
}