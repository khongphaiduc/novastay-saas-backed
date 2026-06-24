using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IPropertyService
{
    Task<IReadOnlyList<PropertyDto>> GetPropertiesByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<PropertyDto> CreatePropertyAsync(Guid organizationId, CreatePropertyRequest request, CancellationToken cancellationToken = default);
    Task<PropertyDto> UpdatePropertyAsync(Guid organizationId, Guid propertyId, UpdatePropertyRequest request, CancellationToken cancellationToken = default);
    Task DeletePropertyAsync(Guid organizationId, Guid propertyId, CancellationToken cancellationToken = default);
}
