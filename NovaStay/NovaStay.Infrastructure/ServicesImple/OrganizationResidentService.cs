using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class OrganizationResidentService : IOrganizationResidentService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IResidentMembershipRepository _residentMembershipRepository;

    public OrganizationResidentService(
        IOrganizationRepository organizationRepository,
        IResidentMembershipRepository residentMembershipRepository)
    {
        _organizationRepository = organizationRepository;
        _residentMembershipRepository = residentMembershipRepository;
    }

    public async Task<IReadOnlyList<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return null;
        }

        return await _residentMembershipRepository.GetResidentsByOrganizationAsync(
            organizationId,
            status,
            cancellationToken);
    }
}
