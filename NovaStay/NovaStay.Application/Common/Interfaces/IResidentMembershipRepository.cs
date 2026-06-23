using NovaStay.Domain.Entities;
using NovaStay.Application.DTOs;

namespace NovaStay.Application.Common.Interfaces;

public interface IResidentMembershipRepository : IRepository<ResidentMembershipEntity>
{
    Task<ResidentMembershipEntity?> GetActiveByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationResidentDto>> GetResidentsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);
}
