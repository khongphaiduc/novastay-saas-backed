using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IResidentService
{
    Task<IReadOnlyList<ResidentDto>> SearchByPhoneAsync(
        string? phone,
        CancellationToken cancellationToken = default);
}
