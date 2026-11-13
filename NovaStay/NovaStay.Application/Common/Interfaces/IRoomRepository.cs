using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IRoomRepository : IRepository<RoomEntity>
{
    Task<PagedResult<RoomDto>> GetPagedRoomsAsync(Guid propertyId, string? search, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoomDto>> GetRoomsWithImagesAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<bool> RoomNumberExistsAsync(Guid propertyId, string roomNumber, Guid? excludeRoomId, CancellationToken cancellationToken = default);

    Task<RoomDto?> GetRoomWithImagesAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);
}