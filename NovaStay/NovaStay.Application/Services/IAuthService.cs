using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IAuthService
{
    Task<RegisterOrganizationAccountResponse> RegisterOrganizationOwnerAsync(RegisterOrganizationAccountRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<LoginBusinessAccountResponse> LoginBusinessOwnerAsync(LoginBusinessAccountRequest request, string? ipAddress, CancellationToken cancellationToken = default);
}
