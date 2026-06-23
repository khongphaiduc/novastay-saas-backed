using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IOrganizationResidentService
{
    Task<IReadOnlyList<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationResidentInvitationDto>?> GetResidentInvitationsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipResponse> InviteResidentAsync(
        Guid organizationId,
        InviteResidentToOrganizationRequest request,
        CancellationToken cancellationToken = default);

    Task<ResidentMembershipResponse> AcceptInvitationAsync(
        Guid accountId,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}
