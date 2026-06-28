using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IContractService
{
    // TASK-036: Xem danh sách hợp đồng
    Task<PagedResult<ContractDetailDto>> GetContractsAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        Guid? residentId = null,
        int pageIndex = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);

    // TASK-031: Tạo hợp đồng thuê
    Task<ContractDetailDto> CreateContractAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken = default);

    // TASK-032: Cập nhật hợp đồng
    Task<ContractDetailDto> UpdateContractAsync(
        Guid contractId,
        UpdateContractRequest request,
        CancellationToken cancellationToken = default);

    // TASK-033: Gia hạn hợp đồng
    Task<ContractDetailDto> RenewContractAsync(
        Guid contractId,
        RenewContractRequest request,
        CancellationToken cancellationToken = default);

    // TASK-034: Thanh lý hợp đồng
    Task<ContractDetailDto> TerminateContractAsync(
        Guid contractId,
        TerminateContractRequest request,
        CancellationToken cancellationToken = default);

    // TASK-035: Upload file hợp đồng
    Task<ContractDetailDto> UploadContractPdfAsync(
        Guid contractId,
        string pdfUrl,
        CancellationToken cancellationToken = default);

    // TASK-038: Theo dõi hợp đồng sắp hết hạn
    Task<IReadOnlyList<ContractDetailDto>> GetExpiringSoonAsync(
        Guid organizationId,
        int daysThreshold = 30,
        CancellationToken cancellationToken = default);

    // TASK-NEW: Gửi thông báo nhắc nhở gia hạn
    Task SendRenewalNotificationAsync(Guid contractId, CancellationToken cancellationToken = default);
}
