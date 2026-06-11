using Host.Domain.Entities;
namespace Host.Application.Common.Interfaces;
public interface ITenantRepository : IRepository<TenantEntity>
{
    Task<TenantEntity?> GetByOwnerEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken = default);
}