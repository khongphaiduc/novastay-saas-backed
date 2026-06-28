namespace NovaStay.Application.DTOs;

public sealed class UpdateResidentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Sdt { get; set; } = string.Empty;
    public string IdentityCardNumber { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Email { get; set; }
}
