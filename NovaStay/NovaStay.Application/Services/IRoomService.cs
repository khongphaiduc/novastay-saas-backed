using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IRoomService
{
    // TASK-011, 012, 013: Danh sách + tìm kiếm + lọc phòng
    Task<PagedResult<RoomDto>> GetRoomsAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        int pageIndex = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);

    // TASK-014: Thêm phòng mới
    Task<RoomDto> CreateRoomAsync(
        CreateRoomRequest request,
        CancellationToken cancellationToken = default);

    // TASK-015: Cập nhật thông tin phòng
    Task<RoomDto> UpdateRoomAsync(
        Guid roomId,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default);

    // TASK-016: Xóa phòng
    Task DeleteRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    // TASK-017: Upload ảnh phòng
    Task<RoomImageDto> UploadRoomImageAsync(
        Guid roomId,
        UploadRoomImageRequest request,
        CancellationToken cancellationToken = default);

    // TASK-018: Cập nhật giá thuê
    Task<RoomDto> UpdateBasePriceAsync(
        Guid roomId,
        UpdateBasePriceRequest request,
        CancellationToken cancellationToken = default);

    // TASK-019: Cập nhật sức chứa
    Task<RoomDto> UpdateMaxOccupantsAsync(
        Guid roomId,
        UpdateMaxOccupantsRequest request,
        CancellationToken cancellationToken = default);

    // TASK-020: Cập nhật tiện ích phòng
    Task<RoomDto> UpdateAmenitiesAsync(
        Guid roomId,
        UpdateAmenitiesRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteRoomImageAsync(
        Guid roomId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}

