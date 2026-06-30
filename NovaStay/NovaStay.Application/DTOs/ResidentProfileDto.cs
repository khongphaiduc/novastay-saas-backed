namespace NovaStay.Application.DTOs;

/// <summary>
/// TASK-053: Hồ sơ đầy đủ của cư dân (dùng cho /api/residents/me)
/// </summary>
public sealed class ResidentProfileDto
{
    public Guid ResidentId { get; set; }
    public Guid AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool MustSetPassword { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Thông tin membership / tòa nhà đang ở
    public Guid? MembershipId { get; set; }
    public string? MembershipCode { get; set; }
    public string? MembershipStatus { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? BusinessName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? OwnerEmail { get; set; }
}
