using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;

namespace NovaStay.Application.Services;

public sealed class SampleDataService : ISampleDataService
{
    private readonly IOrganizationRepository _OrganizationRepository;
    private readonly IMapper _mapper;
    private readonly HostContext _dbcontext;

    public SampleDataService(
        IOrganizationRepository OrganizationRepository,
        IMapper mapper,
        HostContext host
        )
    {
        _OrganizationRepository = OrganizationRepository;
        _mapper = mapper;
        _dbcontext = host;
    }

    public async Task<int> CreatePackageAsync()
    {
        var packages = new List<SubscriptionPackage>
    {
        new()
        {
            Id = Guid.NewGuid(),
            PackageName = "Trial",
            PackageKey = "TRIAL",
            PriceMonthly = 0,
            AllowAiFeatures = false,
            AllowCustomRoles = false,
            MaxRooms = 10,
            MaxProperties = 1,
            TokenGiftMonthly = 5_000
        },

        new()
        {
            Id = Guid.NewGuid(),
            PackageName = "Basic",
            PackageKey = "BASIC",
            PriceMonthly = 199_000,
            AllowAiFeatures = false,
            AllowCustomRoles = false,
            MaxRooms = 50,
            MaxProperties = 1,
            TokenGiftMonthly = 20_000
        },

        new()
        {
            Id = Guid.NewGuid(),
            PackageName = "Pro",
            PackageKey = "PRO",
            PriceMonthly = 499_000,
            AllowAiFeatures = true,
            AllowCustomRoles = true,
            MaxRooms = 150,
            MaxProperties = 3,
            TokenGiftMonthly = 50_000
        },

        new()
        {
            Id = Guid.NewGuid(),
            PackageName = "Premium",
            PackageKey = "PREMIUM",
            PriceMonthly = 999_000,
            AllowAiFeatures = true,
            AllowCustomRoles = true,
            MaxRooms = 500,
            MaxProperties = 10,
            TokenGiftMonthly = 100_000
        }
    };

        var packageKeys = packages
            .Select(package => package.PackageKey)
            .ToList();

        var existingPackageKeys = await _dbcontext.SubscriptionPackages
            .Where(package => packageKeys.Contains(package.PackageKey))
            .Select(package => package.PackageKey)
            .ToListAsync();

        var newPackages = packages
            .Where(package => !existingPackageKeys.Contains(package.PackageKey))
            .ToList();

        if (newPackages.Count == 0)
        {
            return 0;
        }

        await _dbcontext.SubscriptionPackages.AddRangeAsync(newPackages);

        return await _dbcontext.SaveChangesAsync();
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
