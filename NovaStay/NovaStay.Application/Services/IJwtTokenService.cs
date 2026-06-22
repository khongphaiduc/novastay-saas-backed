using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IJwtTokenService
{
    AuthTokenResult CreateTokenPair(
        Guid accountId,
        Guid organizationId,
        string accountType,
        string customerName,
        string phone,
        string? email,
        DateTime issuedAt);

    string HashRefreshToken(string refreshToken);
}
