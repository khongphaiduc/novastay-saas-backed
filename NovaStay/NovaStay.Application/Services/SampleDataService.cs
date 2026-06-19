using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public sealed class SampleDataService : ISampleDataService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public SampleDataService(
        ITenantRepository tenantRepository,
        IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TenantDto>> GetTenantsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(take, 1, 100);
        var tenants = await _tenantRepository.ListAsync(cancellationToken);

        return tenants
            .OrderByDescending(tenant => tenant.CreatedAt)
            .Take(pageSize)
            .Select(tenant => _mapper.Map<TenantDto>(tenant))
            .ToList();
    }
}
