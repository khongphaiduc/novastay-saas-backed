using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class SubscriptionPackageEntity : Entity
{
    public EntityName PackageName { get; set; }
    public Code PackageKey { get; set; }
    public Money PriceMonthly { get; set; }
    public int MaxProperties { get; set; }
    public int MaxRooms { get; set; }
    public int? TokenGiftMonthly { get; set; }
    public bool? AllowCustomRoles { get; set; }
    public bool? AllowAiFeatures { get; set; }
    public DateTime? CreatedAt { get; set; }
}