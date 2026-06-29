using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class PropertyEntity : Entity
{
    public Guid OrganizationId { get; set; }
    public EntityName PropertyName { get; set; }
    public string Address { get; set; } = string.Empty;
    public Code PropertyType { get; set; }
    public Status Status { get; set; } = new("Active");
    public DateTime? CreatedAt { get; set; }
}
