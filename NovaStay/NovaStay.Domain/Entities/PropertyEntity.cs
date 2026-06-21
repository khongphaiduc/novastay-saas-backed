using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class PropertyEntity : Entity
{
    public Guid OrganizationId { get; set; }
    public EntityName PropertyName { get; set; }
    public string Address { get; set; } = string.Empty;
    public Code PropertyType { get; set; }
    public DateTime? CreatedAt { get; set; }
}