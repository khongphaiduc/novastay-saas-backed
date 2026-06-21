namespace NovaStay.Domain.Entities;

public sealed class AccountAccessTokenEntity : Entity
{
    public Guid AccountId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
}
