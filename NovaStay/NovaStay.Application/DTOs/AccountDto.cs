namespace NovaStay.Application.DTOs;

public sealed class AccountDto
{
    public Guid Id { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool? MustSetPassword { get; set; }
    public DateTime? PasswordSetAt { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}
