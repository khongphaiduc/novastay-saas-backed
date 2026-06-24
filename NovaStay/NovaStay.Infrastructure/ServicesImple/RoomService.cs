using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMinioStorageService _storage;

    public RoomService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMinioStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    // TASK-011, 012, 013: Danh sách + tìm kiếm + lọc
    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("propertyId là bắt buộc.");

        return await _unitOfWork.Rooms.GetRoomsWithImagesAsync(propertyId, search, status, cancellationToken);
    }

    // TASK-014: Thêm phòng mới
    public async Task<RoomDto> CreateRoomAsync(
        CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = new RoomEntity
        {
            Id = Guid.NewGuid(),
            PropertyId = request.PropertyId,
            RoomNumber = new Code(request.RoomNumber),
            Floor = request.Floor,
            BasePrice = new Money(request.BasePrice),
            Status = new Status(request.Status),
            MaxOccupants = request.MaxOccupants,
            AmenitiesJson = request.AmenitiesJson,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Rooms.AddAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _unitOfWork.Rooms.GetRoomWithImagesAsync(room.Id, cancellationToken);
        return result ?? _mapper.Map<RoomDto>(room);
    }

    // TASK-015: Cập nhật thông tin phòng
    public async Task<RoomDto> UpdateRoomAsync(
        Guid roomId,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} không tìm thấy.");

        room.RoomNumber = new Code(request.RoomNumber);
        room.Floor = request.Floor;
        room.BasePrice = new Money(request.BasePrice);
        room.Status = new Status(request.Status);
        room.MaxOccupants = request.MaxOccupants;
        room.AmenitiesJson = request.AmenitiesJson;

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _unitOfWork.Rooms.GetRoomWithImagesAsync(roomId, cancellationToken);
        return result ?? _mapper.Map<RoomDto>(room);
    }

    // TASK-016: Xóa phòng (Soft Delete)
    public async Task DeleteRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} không tìm thấy.");

        // Thực hiện Xóa mềm (Soft Delete)
        room.IsDeleted = true;

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // TASK-017: Upload ảnh phòng lên MinIO
    public async Task<RoomImageDto> UploadRoomImageAsync(
        Guid roomId,
        UploadRoomImageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ImageStream == Stream.Null || request.FileSize == 0)
            throw new ArgumentException("Vui lòng chọn file ảnh.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(request.ContentType.ToLower()))
            throw new ArgumentException("Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP.");

        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} không tìm thấy.");

        var imageUrl = await _storage.UploadImageAsync(
            request.ImageStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        // Nếu set IsCover = true, unset tất cả ảnh cover cũ
        if (request.IsCover)
        {
            var existingImages = await _unitOfWork.RoomImages.FindAsync(
                img => img.RoomId == roomId, cancellationToken);

            foreach (var img in existingImages)
            {
                if (img.IsCover == true)
                {
                    img.IsCover = false;
                    _unitOfWork.RoomImages.Update(img);
                }
            }
        }

        var roomImage = new RoomImageEntity
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            ImageUrl = imageUrl,
            IsCover = request.IsCover,
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.RoomImages.AddAsync(roomImage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RoomImageDto>(roomImage);
    }

    // TASK-018: Cập nhật giá thuê
    public async Task<RoomDto> UpdateBasePriceAsync(
        Guid roomId,
        UpdateBasePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} không tìm thấy.");

        room.BasePrice = new Money(request.BasePrice);

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _unitOfWork.Rooms.GetRoomWithImagesAsync(roomId, cancellationToken);
        return result ?? _mapper.Map<RoomDto>(room);
    }

    // TASK-019: Cập nhật sức chứa
    public async Task<RoomDto> UpdateMaxOccupantsAsync(
        Guid roomId,
        UpdateMaxOccupantsRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} không tìm thấy.");

        room.MaxOccupants = request.MaxOccupants;

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _unitOfWork.Rooms.GetRoomWithImagesAsync(roomId, cancellationToken);
        return result ?? _mapper.Map<RoomDto>(room);
    }

    // TASK-020: Cập nhật tiện ích
    public async Task<RoomDto> UpdateAmenitiesAsync(
        Guid roomId,
        UpdateAmenitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} không tìm thấy.");

        room.AmenitiesJson = request.AmenitiesJson;

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _unitOfWork.Rooms.GetRoomWithImagesAsync(roomId, cancellationToken);
        return result ?? _mapper.Map<RoomDto>(room);
    }
}
