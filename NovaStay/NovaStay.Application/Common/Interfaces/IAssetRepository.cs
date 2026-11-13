using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IAssetRepository : IRepository<AssetEntity>
{
    /// <summary>TASK-055: Lấy danh sách tài sản kèm thông tin assignment mới nhất</summary>
    Task<IReadOnlyList<AssetDto>> GetAssetsWithCurrentStatusAsync(
        Guid organizationId,
        string? search = null,
        string? category = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Danh sách tài sản có phân trang và lọc theo vị trí</summary>
    Task<(IReadOnlyList<AssetDto> Items, int TotalCount)> GetAssetsPagedAsync(
        Guid organizationId,
        string? search,
        string? category,
        string? status,
        string? location,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Task phát sinh: Xem tài sản đang trong phòng</summary>
    Task<IReadOnlyList<AssetDto>> GetAssetsByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra mã tài sản đã tồn tại trong cơ sở (không phân biệt hoa thường, bỏ qua tài sản đã xóa)</summary>
    Task<bool> AssetCodeExistsAsync(
        Guid organizationId,
        string assetCode,
        Guid? excludeAssetId,
        CancellationToken cancellationToken = default);

    /// <summary>Task phát sinh: Thống kê tài sản</summary>
    Task<AssetStatisticsDto> GetStatisticsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}