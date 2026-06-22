using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ResidentService : IResidentService
{
    private readonly IResidentRepository _residentRepository;
    private readonly IMapper _mapper;

    public ResidentService(IResidentRepository residentRepository, IMapper mapper)
    {
        _residentRepository = residentRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ResidentDto>> SearchByPhoneAsync(
        string? phone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return [];
        }

        var residents = await _residentRepository.SearchByPhoneAsync(
            phone.Trim(),
            cancellationToken);

        return _mapper.Map<IReadOnlyList<ResidentDto>>(residents);
    }
}
