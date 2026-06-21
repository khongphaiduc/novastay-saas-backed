using System;

namespace NovaStay.Infrastructure.Models;

public partial class AccountRefreshToken
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public virtual Account Account { get; set; } = null!;
}
