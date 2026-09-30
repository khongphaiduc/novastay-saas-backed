using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IAdminService
{
    /// <summary>
    /// Lấy thống kê tổng quan hệ thống
    /// </summary>
    Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách doanh nghiệp với filter và phân trang
    /// </summary>
    Task<IReadOnlyList<AdminOrganizationSummaryDto>> GetOrganizationSummariesAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách cư dân (1 row/người) với filter và phân trang
    /// </summary>
    Task<IReadOnlyList<AdminResidentSummaryDto>> GetResidentSummariesAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);
}
