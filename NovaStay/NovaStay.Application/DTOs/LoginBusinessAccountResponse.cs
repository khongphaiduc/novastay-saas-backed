namespace NovaStay.Application.DTOs;

public sealed class LoginBusinessAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid OrganizationId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BusinessArea { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
