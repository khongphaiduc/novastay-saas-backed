using NovaStay.Domain.Entities;
namespace NovaStay.Application.Common.Interfaces;
public interface ITenantRepository : IRepository<TenantEntity>
{
    Task<TenantEntity?> GetByOwnerEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken = default);
}