namespace NovaStay.Application.DTOs;

public sealed class RegisterResidentAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid ResidentId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
