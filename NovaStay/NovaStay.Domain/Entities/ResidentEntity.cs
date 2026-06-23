using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class ResidentEntity : Entity
{
    public Guid AccountId { get; set; }
    public EntityName FullName { get; set; }
    public string Sex { get; set; } = string.Empty;
    public PhoneNumber Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}
