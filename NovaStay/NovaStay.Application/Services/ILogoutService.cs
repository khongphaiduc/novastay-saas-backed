using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface ILogoutService
{
    Task LogoutAsync(
        LogoutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
