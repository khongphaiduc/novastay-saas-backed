namespace NovaStay.Application.DTOs;

public sealed class SubscriptionPackageDto
{
    public Guid Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageKey { get; set; } = string.Empty;
    public decimal PriceMonthly { get; set; }
    public int MaxProperties { get; set; }
    public int MaxRooms { get; set; }
    public int? TokenGiftMonthly { get; set; }
    public bool? AllowCustomRoles { get; set; }
    public bool? AllowAiFeatures { get; set; }
    public DateTime? CreatedAt { get; set; }
}