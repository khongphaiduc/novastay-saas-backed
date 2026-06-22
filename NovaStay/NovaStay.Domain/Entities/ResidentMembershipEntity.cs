using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class ResidentMembershipEntity : Entity
{
    public Guid AccountId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid OrganizationId { get; set; }
    public string MembershipCode { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}
