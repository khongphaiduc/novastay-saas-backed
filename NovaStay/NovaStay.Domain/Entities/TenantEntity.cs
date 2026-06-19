using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class TenantEntity : Entity
{
    public Guid PackageId { get; set; }
    public EntityName BusinessName { get; set; }
    public string? TaxCode { get; set; }
    public EmailAddress OwnerEmail { get; set; }
    public PhoneNumber OwnerPhone { get; set; }
    public Status SubscriptionStatus { get; set; }
    public int? TokenBalance { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}