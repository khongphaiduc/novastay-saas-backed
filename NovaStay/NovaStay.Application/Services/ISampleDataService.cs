using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface ISampleDataService
{
    Task<IReadOnlyList<OrganizationDto>> GetOrganizationsAsync(
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<int> CreatePackageAsync();
}
