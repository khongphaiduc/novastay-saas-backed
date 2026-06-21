using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public sealed class SampleDataService : ISampleDataService
{
    private readonly IOrganizationRepository _OrganizationRepository;
    private readonly IMapper _mapper;

    public SampleDataService(
        IOrganizationRepository OrganizationRepository,
        IMapper mapper)
    {
        _OrganizationRepository = OrganizationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrganizationDto>> GetOrganizationsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(take, 1, 100);
        var Organizations = await _OrganizationRepository.ListAsync(cancellationToken);

        return Organizations
            .OrderByDescending(Organization => Organization.CreatedAt)
            .Take(pageSize)
            .Select(Organization => _mapper.Map<OrganizationDto>(Organization))
            .ToList();
    }
}
