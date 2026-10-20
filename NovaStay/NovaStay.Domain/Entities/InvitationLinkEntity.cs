namespace NovaStay.Domain.Entities;

public sealed class InvitationLinkEntity : Entity
{
    public Guid OrganizationId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? RoomId { get; set; }

    /// <summary>Chuỗi token ngẫu nhiên dùng trong URL: /invitation/{Token}</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Link hết hạn sau thời điểm này</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Số lượt sử dụng tối đa (0 = không giới hạn)</summary>
    public int MaxUses { get; set; }

    /// <summary>Số lượt đã dùng</summary>
    public int UsedCount { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Ghi chú của chủ trọ (ví dụ: "Phòng 101 tầng 2")</summary>
    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }
}
