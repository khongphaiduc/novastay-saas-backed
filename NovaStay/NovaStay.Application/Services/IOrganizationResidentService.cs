using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IOrganizationResidentService
{
    Task<PagedResponse<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationResidentInvitationDto>?> GetResidentInvitationsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipResponse> InviteResidentAsync(
        Guid organizationId,
        InviteResidentToOrganizationRequest request,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipResponse> AddActiveResidentAsync(
        Guid organizationId,
        Guid residentId,
        CancellationToken cancellationToken = default);

    Task RemoveResidentAsync(
        Guid organizationId,
        Guid residentId,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipResponse> AcceptInvitationAsync(
        Guid accountId,
        Guid membershipId,
        bool isAccepted = true,
        CancellationToken cancellationToken = default);

    Task CancelInvitationAsync(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}
