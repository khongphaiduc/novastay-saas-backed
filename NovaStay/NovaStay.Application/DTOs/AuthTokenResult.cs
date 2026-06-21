namespace NovaStay.Application.DTOs;

public sealed class AuthTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
