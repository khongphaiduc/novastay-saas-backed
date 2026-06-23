using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ResidentInvitationService : IResidentInvitationService
{
    private readonly IResidentMembershipRepository _residentMembershipRepository;

    public ResidentInvitationService(IResidentMembershipRepository residentMembershipRepository)
    {
        _residentMembershipRepository = residentMembershipRepository;
    }

    public async Task<IReadOnlyList<ResidentInvitationDto>> GetPendingInvitationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        return await _residentMembershipRepository.GetPendingInvitationsByAccountIdAsync(
            accountId,
            cancellationToken);
    }
}
