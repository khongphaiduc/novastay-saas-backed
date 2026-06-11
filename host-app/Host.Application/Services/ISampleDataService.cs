using Host.Application.DTOs;

namespace Host.Application.Services;

public interface ISampleDataService
{
    Task<IReadOnlyList<TenantDto>> GetTenantsAsync(
        int take = 20,
        CancellationToken cancellationToken = default);
}
