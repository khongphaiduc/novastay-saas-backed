using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IPropertyService
{
    Task<IReadOnlyList<PropertyDto>> GetPropertiesByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
