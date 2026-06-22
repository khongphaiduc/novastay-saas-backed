namespace NovaStay.Application.DTOs;

public sealed class ResidentDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}
