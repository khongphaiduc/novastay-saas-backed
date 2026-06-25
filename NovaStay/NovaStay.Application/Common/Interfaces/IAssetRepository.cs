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

    /// <summary>Task phát sinh: Xem tài sản đang trong phòng</summary>
    Task<IReadOnlyList<AssetDto>> GetAssetsByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>Task phát sinh: Thống kê tài sản</summary>
    Task<AssetStatisticsDto> GetStatisticsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}