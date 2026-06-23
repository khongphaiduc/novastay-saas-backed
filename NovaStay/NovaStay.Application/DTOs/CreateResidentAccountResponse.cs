namespace NovaStay.Application.DTOs;

public sealed class CreateResidentAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid ResidentId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sdt { get; set; } = string.Empty;
    public string IdentityCardNumber { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool MustSetPassword { get; set; }
}
