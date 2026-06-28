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

    Task<IReadOnlyList<ResidentAccommodationDto>> GetActiveAccommodationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    Task<ResidentDto> UpdateResidentAsync(
        Guid residentId,
        UpdateResidentRequest request,
        CancellationToken cancellationToken = default);

    Task<ResidentDto> UploadImageAsync(
        Guid residentId,
        string imageType,
        string imageUrl,
        CancellationToken cancellationToken = default);
}
