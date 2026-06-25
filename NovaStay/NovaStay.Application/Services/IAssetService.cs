using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IAssetService
{
    // TASK-055: Xem danh sách tài sản
    Task<IReadOnlyList<AssetDto>> GetAssetsAsync(
        Guid organizationId,
        string? search = null,
        string? category = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    // TASK-055 (derived): Xem tài sản trong một phòng
    Task<IReadOnlyList<AssetDto>> GetAssetsByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    // TASK-056: Thêm tài sản mới
    Task<AssetDto> CreateAssetAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default);

    // TASK-056: Xóa tài sản (soft delete)
    Task DeleteAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    // TASK-057: Cập nhật thông tin tài sản
    Task<AssetDto> UpdateAssetAsync(
        Guid assetId,
        UpdateAssetRequest request,
        CancellationToken cancellationToken = default);

    // TASK-058: Gán tài sản vào phòng
    Task<AssetHistoryDto> AssignAssetToRoomAsync(
        Guid assetId,
        AssignAssetRequest request,
        CancellationToken cancellationToken = default);

    // TASK-059: Thu hồi tài sản khỏi phòng (đưa về kho)
    Task<AssetHistoryDto> RevokeAssetFromRoomAsync(
        Guid assetId,
        RevokeAssetRequest request,
        CancellationToken cancellationToken = default);

    // TASK-060 & TASK-061: Cập nhật tình trạng / Ghi nhận hư hỏng tài sản
    Task<AssetHistoryDto> UpdateAssetStatusAsync(
        Guid assetId,
        UpdateAssetStatusRequest request,
        CancellationToken cancellationToken = default);

    // TASK-062: Xem lịch sử luân chuyển tài sản
    Task<IReadOnlyList<AssetHistoryDto>> GetAssetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    // Task phát sinh: Thống kê tài sản
    Task<AssetStatisticsDto> GetStatisticsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
