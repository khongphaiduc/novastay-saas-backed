using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IResidentService
{
    Task<CreateResidentAccountResponse> CreateResidentAccountAsync(
        CreateResidentAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResidentDto>> SearchByPhoneAsync(
        string? phone,
        CancellationToken cancellationToken = default);
}
