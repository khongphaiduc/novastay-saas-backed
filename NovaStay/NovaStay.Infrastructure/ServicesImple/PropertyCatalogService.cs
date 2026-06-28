using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class PropertyCatalogService : IPropertyCatalogService
{
    private readonly IPropertyServiceRepository _propertyServiceRepository;

    public PropertyCatalogService(IPropertyServiceRepository propertyServiceRepository)
    {
        _propertyServiceRepository = propertyServiceRepository;
    }

    public Task<IReadOnlyList<PropertyServiceDto>?> GetPropertyServicesAsync(
        Guid organizationId,
        Guid propertyId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        return _propertyServiceRepository.GetPropertyServicesAsync(
            organizationId,
            propertyId,
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            cancellationToken);
    }

    public Task<PropertyServiceDto> CreatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        CreatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ServiceName))
        {
            throw new ArgumentException("ServiceName is required.");
        }

        if (request.DefaultPrice < 0)
        {
            throw new ArgumentException("DefaultPrice must be greater than or equal to 0.");
        }

        request.ServiceName = request.ServiceName.Trim();
        request.ServiceCode = string.IsNullOrWhiteSpace(request.ServiceCode) ? null : request.ServiceCode.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        request.Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim();
        request.BillingCycle = string.IsNullOrWhiteSpace(request.BillingCycle) ? null : request.BillingCycle.Trim();

        return _propertyServiceRepository.CreatePropertyServiceAsync(
            organizationId,
            propertyId,
            request,
            cancellationToken);
    }

    public Task<PropertyServiceDto> UpdatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        Guid propertyServiceId,
        UpdatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DefaultPrice < 0)
        {
            throw new ArgumentException("DefaultPrice must be greater than or equal to 0.");
        }

        return _propertyServiceRepository.UpdatePropertyServiceAsync(
            organizationId,
            propertyId,
            propertyServiceId,
            request.DefaultPrice,
            request.BillingCycle,
            request.IsActive,
            cancellationToken);
    }
}
