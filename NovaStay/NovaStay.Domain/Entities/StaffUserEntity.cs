using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class StaffUserEntity : Entity
{
    public Guid AccountId { get; set; }
    public Guid OrganizationId { get; set; }
    public EntityName FullName { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
