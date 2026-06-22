namespace NovaStay.Application.DTOs;

public sealed class OrganizationResidentDto
{
    public Guid MembershipId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid AccountId { get; set; }
    public string MembershipCode { get; set; } = string.Empty;
    public string MembershipStatus { get; set; } = string.Empty;
    public DateTime? InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool? AccountIsActive { get; set; }
    public bool? MustSetPassword { get; set; }
}
