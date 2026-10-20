using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed class CreateInvitationLinkRequest
{
    public Guid? PropertyId { get; set; }
    public Guid? RoomId { get; set; }

    /// <summary>Số ngày link còn hiệu lực (mặc định 7 ngày)</summary>
    [Range(1, 90)]
    public int ExpiresInDays { get; set; } = 7;

    /// <summary>Số lượt tối đa (0 = không giới hạn)</summary>
    [Range(0, 1000)]
    public int MaxUses { get; set; } = 0;

    [MaxLength(255)]
    public string? Note { get; set; }
}

public sealed class InvitationLinkDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? RoomId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string InviteUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int MaxUses { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? PropertyName { get; set; }
    public string? RoomName { get; set; }
}

public sealed class AcceptInvitationLinkRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public sealed class InvitationLinkPreviewDto
{
    public string Token { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public string? RoomName { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsValid { get; set; }
    public string? InvalidReason { get; set; }
}

public sealed class PendingResidentApprovalDto
{
    public Guid MembershipId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid AccountId { get; set; }
    public string MembershipCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? RequestedAt { get; set; }
}

