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

    public async Task<PagedResult<PropertyDto>> GetPropertiesAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        int pageIndex = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var allProperties = await _unitOfWork.Properties.FindAsync(p => p.OrganizationId == organizationId, cancellationToken);
        
        // In memory filtering
        var query = allProperties.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => 
                p.PropertyName.Value.ToLower().Contains(s) || 
                p.Address.ToLower().Contains(s));
        }

        // No status filter for Property as PropertyEntity doesn't have a Status property.

        var totalCount = query.Count();
        var pagedItems = query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(_mapper.Map<PropertyDto>)
            .ToList();

        return new PagedResult<PropertyDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
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
