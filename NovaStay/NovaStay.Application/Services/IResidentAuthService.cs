using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IResidentAuthService
{
    Task<LoginResidentAccountResponse> LoginAsync(
        LoginResidentAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<RegisterResidentAccountResponse> RegisterAsync(
        RegisterResidentAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<LoginResidentAccountResponse> GoogleLoginAsync(
        GoogleLoginResidentRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
