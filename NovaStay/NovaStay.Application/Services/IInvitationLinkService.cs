using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IInvitationLinkService
{
    /// <summary>Chủ trọ tạo link mời</summary>
    Task<InvitationLinkDto> CreateAsync(
        Guid organizationId,
        CreateInvitationLinkRequest request,
        string baseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Chủ trọ xem danh sách link đã tạo</summary>
    Task<IReadOnlyList<InvitationLinkDto>> GetByOrganizationAsync(
        Guid organizationId,
        string baseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Chủ trọ huỷ / vô hiệu hoá link</summary>
    Task DeactivateAsync(
        Guid organizationId,
        Guid linkId,
        CancellationToken cancellationToken = default);

    /// <summary>Người thuê xem preview trước khi chấp nhận</summary>
    Task<InvitationLinkPreviewDto> GetPreviewAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>Người thuê (đã đăng nhập) dùng link để xin gia nhập Organization</summary>
    Task<ResidentMembershipResponse> AcceptAsync(
        Guid accountId,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>Chủ trọ xem danh sách người thuê đang chờ duyệt</summary>
    Task<IReadOnlyList<PendingResidentApprovalDto>> GetPendingApprovalsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>Chủ trọ duyệt người thuê</summary>
    Task<ResidentMembershipResponse> ApprovePendingResidentAsync(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    /// <summary>Chủ trọ từ chối người thuê</summary>
    Task<ResidentMembershipResponse> RejectPendingResidentAsync(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}
