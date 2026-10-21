using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IInvitationLinkRepository : IRepository<InvitationLinkEntity>
{
    Task<InvitationLinkEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvitationLinkEntity>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
