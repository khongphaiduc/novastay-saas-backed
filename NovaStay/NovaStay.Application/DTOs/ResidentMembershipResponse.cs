namespace NovaStay.Application.DTOs;

public sealed class ResidentMembershipResponse
{
    public Guid MembershipId { get; set; }
    public Guid AccountId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid OrganizationId { get; set; }
    public string MembershipCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}
