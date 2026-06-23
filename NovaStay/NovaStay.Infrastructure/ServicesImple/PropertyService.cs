using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class PropertyService : IPropertyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PropertyService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PropertyDto>> GetPropertiesByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var properties = await _unitOfWork.Properties.FindAsync(p => p.OrganizationId == organizationId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PropertyDto>>(properties);
    }
}
