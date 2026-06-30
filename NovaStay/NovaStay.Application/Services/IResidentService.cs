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

    // TASK-053: Lấy hồ sơ đầy đủ của cư dân (Resident token)
    Task<ResidentProfileDto> GetMyProfileAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    // TASK-049: Lấy thông tin phòng đang ở + ảnh
    Task<ResidentRoomDto?> GetMyRoomAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    // TASK-051: Lấy danh sách hợp đồng của cư dân
    Task<IReadOnlyList<ContractDetailDto>> GetMyContractsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    // TASK-052: Lấy danh sách hóa đơn của cư dân
    Task<IReadOnlyList<IncomeReceiptDto>> GetMyInvoicesAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    // TASK-053: Cư dân tự cập nhật hồ sơ cá nhân
    Task<ResidentProfileDto> UpdateMyProfileAsync(
        Guid accountId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default);

    // TASK-053: Cư dân tự upload ảnh đại diện
    Task<string> UploadMyAvatarAsync(
        Guid accountId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
