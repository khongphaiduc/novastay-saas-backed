using NovaStay.Domain.Entities;
using NovaStay.Application.DTOs;

namespace NovaStay.Application.Common.Interfaces;

public interface IResidentMembershipRepository : IRepository<ResidentMembershipEntity>
{
    Task<ResidentMembershipEntity?> GetActiveByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipEntity?> GetByResidentAndOrganizationAsync(
        Guid residentId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipEntity?> GetByIdForAccountAsync(
        Guid membershipId,
        Guid accountId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByMembershipCodeAsync(
        string membershipCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationResidentDto>> GetResidentsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationResidentInvitationDto>> GetInvitationsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResidentAccommodationDto>> GetActiveAccommodationsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
}
