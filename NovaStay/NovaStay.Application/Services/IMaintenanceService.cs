using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IMaintenanceService
{
    // TASK-021: Xem lịch sử bảo trì của phòng
    Task<PagedResult<MaintenanceTicketDetailDto>> GetByRoomAsync(
        Guid roomId,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    // TASK-022: Đánh dấu phòng đang bảo trì / trống
    Task<string> MarkRoomStatusAsync(
        Guid roomId,
        string newStatus,
        Guid organizationId,
        Guid? residentId,
        string? description = null,
        CancellationToken cancellationToken = default);

    // TASK-021: Tạo phiếu bảo trì
    Task<MaintenanceTicketDetailDto> CreateTicketAsync(
        CreateMaintenanceTicketRequest request,
        CancellationToken cancellationToken = default);

    // Chủ nhà xem phiếu sự cố / bảo trì của cơ sở
    Task<IReadOnlyList<MaintenanceTicketDetailDto>> GetOrganizationTicketsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    // Chủ nhà đổi trạng thái phiếu (Pending -> InProgress -> Resolved)
    Task<MaintenanceTicketDetailDto> UpdateTicketStatusAsync(
        Guid ticketId,
        string status,
        CancellationToken cancellationToken = default);
}
