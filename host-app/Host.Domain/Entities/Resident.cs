using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Resident : Entity
{
    public Guid TenantId { get; set; }
    public EntityName FullName { get; set; }
    public PhoneNumber Phone { get; set; }
    public string? Email { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}