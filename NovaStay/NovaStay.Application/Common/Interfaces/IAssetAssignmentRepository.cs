using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IAssetAssignmentRepository : IRepository<AssetAssignmentEntity>
{
    /// <summary>TASK-062: Lấy toàn bộ lịch sử luân chuyển của 1 tài sản</summary>
    Task<IReadOnlyList<AssetHistoryDto>> GetHistoryByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    /// <summary>Lấy bản ghi assignment mới nhất của tài sản</summary>
    Task<AssetAssignmentEntity?> GetLatestByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);
}