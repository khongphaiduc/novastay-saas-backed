using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.Persistence.Auth;

internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokenResult CreateTokenPair(
        Guid accountId,
        Guid organizationId,
        string accountType,
        string customerName,
        string phone,
        string? email,
        DateTime issuedAt)
    {
        var accessTokenExpiresAt = issuedAt.AddMinutes(GetJwtInt("AccessTokenMinutes", 60));
        var refreshTokenExpiresAt = issuedAt.AddDays(GetJwtInt("RefreshTokenDays", 30));
        var refreshToken = CreateSecureToken();

        return new AuthTokenResult
        {
            AccessToken = CreateAccessToken(
                accountId,
                organizationId,
                accountType,
                customerName,
                phone,
                email,
                accessTokenExpiresAt),
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenHash = HashToken(refreshToken),
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    private string CreateAccessToken(
        Guid accountId,
        Guid organizationId,
        string accountType,
        string customerName,
        string phone,
        string? email,
        DateTime expiresAt)
    {
        var secret = GetJwtString("Secret");
        var issuer = GetJwtString("Issuer");
        var audience = GetJwtString("Audience");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, customerName),
            new Claim(ClaimTypes.Role, accountType),
            new Claim("accountType", accountType),
            new Claim("organizationId", organizationId.ToString()),
            new Claim("phone", phone)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetJwtString(string key)
    {
        var value = _configuration[$"Jwt:{key}"];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Jwt:{key} is not configured.");
        }

        return value;
    }

    private int GetJwtInt(string key, int fallback)
    {
        return int.TryParse(_configuration[$"Jwt:{key}"], out var value)
            ? value
            : fallback;
    }

    private static string CreateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
