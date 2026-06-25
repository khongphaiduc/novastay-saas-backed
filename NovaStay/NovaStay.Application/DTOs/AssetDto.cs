namespace NovaStay.Application.DTOs;

/// <summary>TASK-055: DTO hiển thị tài sản kèm trạng thái & phòng hiện tại</summary>
public sealed class AssetDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetCode { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public decimal? BaseValue { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Thông tin assignment hiện tại (latest)
    public Guid? CurrentAssignmentId { get; set; }
    public Guid? CurrentRoomId { get; set; }
    public string? CurrentRoomNumber { get; set; }
    public string? CurrentStatus { get; set; }
    public string? CurrentNote { get; set; }
    public DateTime? LastAssignedAt { get; set; }
}

/// <summary>TASK-062: DTO cho 1 bản ghi lịch sử luân chuyển tài sản</summary>
public sealed class AssetHistoryDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public DateTime? AssignedAt { get; set; }
}

/// <summary>Task phát sinh: DTO thống kê tài sản</summary>
public sealed class AssetStatisticsDto
{
    public int TotalAssets { get; set; }
    public int InStorage { get; set; }     // Đang trong kho (không gán phòng)
    public int InUse { get; set; }         // Đang sử dụng (gán phòng, status Good/Working)
    public int Damaged { get; set; }       // Đang hỏng
    public int InMaintenance { get; set; } // Đang bảo trì
}