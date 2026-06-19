using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class TechnicianEntity : Entity
{
    public Guid TenantId { get; set; }
    public EntityName FullName { get; set; }
    public PhoneNumber Phone { get; set; }
    public Code Specialty { get; set; }
    public bool? IsAvailable { get; set; }
}