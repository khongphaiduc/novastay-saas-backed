using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IOrganizationResidentService
{
    Task<IReadOnlyList<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);
}
