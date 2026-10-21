namespace NovaStay.Infrastructure.Models;

public partial class InvitationLink
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? RoomId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public int MaxUses { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual Property? Property { get; set; }
    public virtual Room? Room { get; set; }
}
