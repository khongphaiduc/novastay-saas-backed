using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IRoomRepository : IRepository<RoomEntity>
{
    Task<IReadOnlyList<RoomDto>> GetRoomsWithImagesAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<RoomDto?> GetRoomWithImagesAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);
}