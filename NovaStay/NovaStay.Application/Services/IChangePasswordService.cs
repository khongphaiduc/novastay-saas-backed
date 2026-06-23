using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IChangePasswordService
{
    Task ChangePasswordAsync(
        Guid accountId,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
