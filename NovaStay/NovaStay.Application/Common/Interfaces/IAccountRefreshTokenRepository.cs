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
