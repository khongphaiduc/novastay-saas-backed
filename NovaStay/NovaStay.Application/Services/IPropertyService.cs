using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IPropertyService
{
    Task<PagedResult<PropertyDto>> GetPropertiesAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        int pageIndex = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);
    Task<PropertyDto> CreatePropertyAsync(Guid organizationId, CreatePropertyRequest request, CancellationToken cancellationToken = default);
    Task<PropertyDto> UpdatePropertyAsync(Guid organizationId, Guid propertyId, UpdatePropertyRequest request, CancellationToken cancellationToken = default);
    Task DeletePropertyAsync(Guid organizationId, Guid propertyId, CancellationToken cancellationToken = default);
}
