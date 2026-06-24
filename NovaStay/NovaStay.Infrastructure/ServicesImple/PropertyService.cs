using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;

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

    public async Task<PropertyDto> CreatePropertyAsync(Guid organizationId, CreatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var property = new PropertyEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyName = new EntityName(request.PropertyName),
            Address = request.Address,
            PropertyType = new Code(request.PropertyType),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Properties.AddAsync(property, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PropertyDto>(property);
    }

    public async Task<PropertyDto> UpdatePropertyAsync(Guid organizationId, Guid propertyId, UpdatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var properties = await _unitOfWork.Properties.FindAsync(p => p.Id == propertyId && p.OrganizationId == organizationId, cancellationToken);
        var property = properties.FirstOrDefault();

        if (property == null)
            throw new KeyNotFoundException($"Property with ID {propertyId} not found or does not belong to the organization.");

        property.PropertyName = new EntityName(request.PropertyName);
        property.Address = request.Address;
        property.PropertyType = new Code(request.PropertyType);

        _unitOfWork.Properties.Update(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PropertyDto>(property);
    }

    public async Task DeletePropertyAsync(Guid organizationId, Guid propertyId, CancellationToken cancellationToken = default)
    {
        var properties = await _unitOfWork.Properties.FindAsync(p => p.Id == propertyId && p.OrganizationId == organizationId, cancellationToken);
        var property = properties.FirstOrDefault();

        if (property == null)
            throw new KeyNotFoundException($"Property with ID {propertyId} not found or does not belong to the organization.");

        var rooms = await _unitOfWork.Rooms.FindAsync(r => r.PropertyId == propertyId, cancellationToken);
        if (rooms.Any())
            throw new InvalidOperationException("Không thể xóa cơ sở vì vẫn còn phòng trọ thuộc cơ sở này. Vui lòng chuyển hoặc xóa hết các phòng trước.");

        _unitOfWork.Properties.Remove(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
