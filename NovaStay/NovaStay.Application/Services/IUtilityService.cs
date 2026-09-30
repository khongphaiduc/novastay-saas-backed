using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IUtilityService
{
    /// <summary>Lấy tất cả đồng hồ của một phòng (kèm chỉ số mới nhất)</summary>
    Task<IReadOnlyList<UtilityMeterDto>> GetMetersByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>Lấy tất cả đồng hồ của một property (kèm chỉ số mới nhất) – dùng để hiển thị danh sách phòng cần ghi số</summary>
    Task<IReadOnlyList<UtilityMeterDto>> GetMetersByPropertyAsync(
        Guid propertyId,
        CancellationToken cancellationToken = default);

    /// <summary>Ghi chỉ số mới cho một đồng hồ</summary>
    Task<UtilityReadingDto> RecordReadingAsync(
        Guid meterId,
        RecordUtilityReadingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Lấy lịch sử ghi số của một đồng hồ</summary>
    Task<IReadOnlyList<UtilityReadingDto>> GetReadingsByMeterAsync(
        Guid meterId,
        CancellationToken cancellationToken = default);

    /// <summary>Tạo mới đồng hồ dịch vụ cho phòng</summary>
    Task<UtilityMeterDto> CreateMeterAsync(
        CreateUtilityMeterRequest request,
        CancellationToken cancellationToken = default);
}
