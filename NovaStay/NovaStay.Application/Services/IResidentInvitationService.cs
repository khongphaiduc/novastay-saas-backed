using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IResidentInvitationService
{
    Task<IReadOnlyList<ResidentInvitationDto>> GetPendingInvitationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
}
