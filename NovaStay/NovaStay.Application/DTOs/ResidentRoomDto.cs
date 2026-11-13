namespace NovaStay.Application.DTOs;

/// <summary>
/// TASK-049: Thông tin phòng đang ở của cư dân + danh sách ảnh
/// </summary>
public sealed class ResidentRoomDto
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal BasePrice { get; set; }
    public int? MaxOccupants { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AmenitiesJson { get; set; }
    public IReadOnlyList<RoomImageDto> Images { get; set; } = [];

    // Tài sản đang được gán vào phòng (không gồm giá trị tài chính)
    public IReadOnlyList<RoomAssetDto> Assets { get; set; } = [];

    // Thông tin tòa nhà chứa phòng
    public Guid PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyAddress { get; set; }
}

/// <summary>Tài sản trong phòng hiển thị cho cư dân</summary>
public sealed class RoomAssetDto
{
    public Guid Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetCode { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public DateTime? LastAssignedAt { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
}
