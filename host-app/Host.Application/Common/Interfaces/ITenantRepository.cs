using Host.Domain.Entities;
namespace Host.Application.Common.Interfaces;
public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByOwnerEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken = default);
}