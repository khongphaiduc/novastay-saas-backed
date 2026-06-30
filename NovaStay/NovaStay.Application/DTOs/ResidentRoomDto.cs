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

    // Thông tin tòa nhà chứa phòng
    public Guid PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyAddress { get; set; }
}
