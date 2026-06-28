using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IMaintenanceTicketRepository : IRepository<MaintenanceTicketEntity>
{
    Task<(IReadOnlyList<MaintenanceTicketDetailDto> Items, int TotalCount)> GetByRoomIdAsync(
        Guid roomId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenanceTicketDetailDto>> GetByOrganizationIdAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);
}