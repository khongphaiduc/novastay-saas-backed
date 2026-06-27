using NovaStay.Application.DTOs;

namespace NovaStay.Application.Common.Interfaces;

public interface IPropertyServiceRepository
{
    Task<IReadOnlyList<PropertyServiceDto>?> GetPropertyServicesAsync(
        Guid organizationId,
        Guid propertyId,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<PropertyServiceDto> CreatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        CreatePropertyServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<PropertyServiceDto> UpdatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        Guid propertyServiceId,
        decimal defaultPrice,
        string? billingCycle,
        bool isActive,
        CancellationToken cancellationToken = default);
}
