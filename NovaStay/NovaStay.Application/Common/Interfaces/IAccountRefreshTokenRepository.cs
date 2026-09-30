using NovaStay.Domain.Entities;
namespace NovaStay.Application.Common.Interfaces;
public interface IAccountRefreshTokenRepository : IRepository<AccountRefreshTokenEntity>
{
    Task<bool> RevokeByTokenHashAsync(
        string tokenHash,
        DateTime revokedAt,
        string? revokedByIp,
        CancellationToken cancellationToken = default);

    Task<int> RevokeActiveByAccountIdAsync(
        Guid accountId,
        DateTime revokedAt,
        string? revokedByIp,
        CancellationToken cancellationToken = default);

}

public class InforUser
{
    public Guid AccountId { get; set; }

    public Guid OrganizationId { get; set; }

    public string AccountType { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

}
