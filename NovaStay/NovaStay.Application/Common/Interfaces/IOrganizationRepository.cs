using NovaStay.Domain.Entities;
namespace NovaStay.Application.Common.Interfaces;
public interface IOrganizationRepository : IRepository<OrganizationEntity>
{
    Task<OrganizationEntity?> GetByOwnerEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken = default);
}