namespace NovaStay.Application.DTOs;

public sealed class LoginResidentAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid OrganizationId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sdt { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool MustSetPassword { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
