using NovaStay.Application.Common;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using System.Globalization;
using System.Security.Cryptography;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class OrganizationResidentService : IOrganizationResidentService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IResidentMembershipRepository _residentMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationResidentService(
        IOrganizationRepository organizationRepository,
        IResidentMembershipRepository residentMembershipRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _residentMembershipRepository = residentMembershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Pagination.Offset(page, pageSize);
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return null;
        }

        var result = await _residentMembershipRepository.GetResidentsByOrganizationAsync(
            organizationId,
            status,
            search,
            page,
            pageSize,
            cancellationToken);

        return new PagedResponse<OrganizationResidentDto>
        {
            Data = result.Data,
            TotalRecords = result.TotalRecords,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<OrganizationResidentInvitationDto>?> GetResidentInvitationsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return null;
        }

        return await _residentMembershipRepository.GetInvitationsByOrganizationAsync(
            organizationId,
            status,
            cancellationToken);
    }

    public async Task<ResidentMembershipResponse> InviteResidentAsync(
        Guid organizationId,
        InviteResidentToOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ResidentId == Guid.Empty)
        {
            throw new InvalidOperationException("ResidentId is required.");
        }

        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null)
        {
            throw new KeyNotFoundException("Organization not found.");
        }

        var resident = await _unitOfWork.Residents.GetByIdAsync(request.ResidentId, cancellationToken);
        if (resident is null)
        {
            throw new KeyNotFoundException("Resident not found.");
        }

        var existingMembership = await _unitOfWork.ResidentMemberships.GetByResidentAndOrganizationAsync(
            request.ResidentId,
            organizationId,
            cancellationToken);

        var now = DateTime.UtcNow;

        // Đã có dòng membership (đang chờ, đã xóa khỏi cơ sở, đã từ chối...): dùng lại dòng đó vì có unique index (AccountId, OrganizationId).
        if (existingMembership is not null)
        {
            if (IsActiveMembership(existingMembership.Status.Value))
            {
                throw new InvalidOperationException("Cư dân này đã là thành viên đang hoạt động của cơ sở này.");
            }

            // Đang chờ phản hồi: gửi lại (làm mới thời gian mời). Đã xóa/đã từ chối: mời lại từ đầu.
            existingMembership.Status = ResidentMembershipStatuses.Pending;
            existingMembership.InvitedAt = now;
            existingMembership.RespondedAt = null;
            existingMembership.JoinedAt = null;
            existingMembership.ActivatedAt = null;
            _unitOfWork.ResidentMemberships.Update(existingMembership);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ToResponse(existingMembership);
        }

        var membership = new ResidentMembershipEntity
        {
            Id = Guid.NewGuid(),
            AccountId = resident.AccountId,
            ResidentId = resident.Id,
            OrganizationId = organization.Id,
            MembershipCode = await CreateUniqueMembershipCodeAsync(cancellationToken),
            Status = ResidentMembershipStatuses.Pending,
            InvitedAt = now,
            CreatedAt = now
        };

        await _unitOfWork.ResidentMemberships.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(membership);
    }

    public async Task<ResidentMembershipResponse> AddActiveResidentAsync(
        Guid organizationId,
        Guid residentId,
        CancellationToken cancellationToken = default)
    {
        if (residentId == Guid.Empty)
        {
            throw new InvalidOperationException("ResidentId is required.");
        }

        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null)
        {
            throw new KeyNotFoundException("Organization not found.");
        }

        var resident = await _unitOfWork.Residents.GetByIdAsync(residentId, cancellationToken);
        if (resident is null)
        {
            throw new KeyNotFoundException("Resident not found.");
        }

        var existingMembership = await _unitOfWork.ResidentMemberships.GetByResidentAndOrganizationAsync(
            residentId,
            organizationId,
            cancellationToken);
        var now = DateTime.UtcNow;
        if (existingMembership is not null)
        {
            if (IsActiveMembership(existingMembership.Status.Value))
            {
                throw new InvalidOperationException("Cư dân này đã là thành viên đang hoạt động của cơ sở này.");
            }

            // Đã từng xóa khỏi cơ sở hoặc từ chối: kích hoạt lại dòng cũ (không tạo dòng mới vì unique index).
            existingMembership.Status = ResidentMembershipStatuses.Active;
            existingMembership.InvitedAt = now;
            existingMembership.RespondedAt = now;
            existingMembership.JoinedAt = now;
            existingMembership.ActivatedAt = now;
            _unitOfWork.ResidentMemberships.Update(existingMembership);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ToResponse(existingMembership);
        }

        var membership = new ResidentMembershipEntity
        {
            Id = Guid.NewGuid(),
            AccountId = resident.AccountId,
            ResidentId = resident.Id,
            OrganizationId = organization.Id,
            MembershipCode = await CreateUniqueMembershipCodeAsync(cancellationToken),
            Status = ResidentMembershipStatuses.Active,
            InvitedAt = now,
            RespondedAt = now,
            JoinedAt = now,
            ActivatedAt = now,
            CreatedAt = now
        };

        await _unitOfWork.ResidentMemberships.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(membership);
    }

    private static bool IsActiveMembership(string status) =>
        string.Equals(status, ResidentMembershipStatuses.Active, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, ResidentMembershipStatuses.LegacyActive, StringComparison.OrdinalIgnoreCase);

    public async Task RemoveResidentAsync(
        Guid organizationId,
        Guid residentId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _unitOfWork.ResidentMemberships.GetByResidentAndOrganizationAsync(
            residentId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            throw new KeyNotFoundException("Không tìm thấy cư dân trong cơ sở này.");
        }

        if (string.Equals(membership.Status.Value, "INACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException("Cư dân này đã được xóa khỏi cơ sở.");
        }

        // Ràng buộc 1: hợp đồng đang hiệu lực (đại diện hoặc ở ghép) trong cơ sở này
        var activeContract = await _unitOfWork.Contracts.FindActiveContractOfResidentAsync(organizationId, residentId, cancellationToken);
        if (activeContract is not null)
        {
            var roomNumber = activeContract.RoomNumber ?? "không xác định";
            var endDate = activeContract.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            throw new InvalidOperationException(activeContract.ResidentId == residentId
                ? $"Không thể xóa cư dân này: đang là người đại diện của hợp đồng hiệu lực ở phòng {roomNumber} (kết thúc ngày {endDate}). Hãy thanh lý hoặc kết thúc hợp đồng đó trước."
                : $"Không thể xóa cư dân này: đang ở ghép trong hợp đồng hiệu lực ở phòng {roomNumber} (kết thúc ngày {endDate}). Hãy gỡ cư dân khỏi danh sách ở ghép của hợp đồng đó trước.");
        }

        // Ràng buộc 2: hóa đơn còn nợ (chưa thanh toán hoặc đang chờ xác nhận)
        var openInvoices = await _unitOfWork.Invoices.GetOpenInvoiceSummaryForResidentAsync(organizationId, residentId, cancellationToken);
        if (openInvoices.Count > 0)
        {
            var total = openInvoices.Total.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
            throw new InvalidOperationException(
                $"Không thể xóa cư dân này: còn {openInvoices.Count} hóa đơn chưa thanh toán hoặc đang chờ xác nhận (tổng {total} đ). Hãy thu xong hoặc xác nhận các hóa đơn đó trước.");
        }

        membership.Status = "INACTIVE";
        _unitOfWork.ResidentMemberships.Update(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ResidentMembershipResponse> AcceptInvitationAsync(
        Guid accountId,
        Guid membershipId,
        bool isAccepted = true,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        var membership = await _unitOfWork.ResidentMemberships.GetByIdForAccountAsync(
            membershipId,
            accountId,
            cancellationToken);
        if (membership is null)
        {
            throw new KeyNotFoundException("Resident membership invitation not found.");
        }

        if (isAccepted && IsActiveStatus(membership.Status.Value))
        {
            return ToResponse(membership);
        }

        if (!isAccepted && membership.Status.Value == ResidentMembershipStatuses.Rejected)
        {
            return ToResponse(membership);
        }

        if (membership.Status.Value != ResidentMembershipStatuses.Pending)
        {
            throw new InvalidOperationException("Only pending invitations can be accepted or rejected.");
        }

        var now = DateTime.UtcNow;
        membership.RespondedAt = now;

        if (isAccepted)
        {
            membership.Status = ResidentMembershipStatuses.Active;
            membership.JoinedAt ??= now;
            membership.ActivatedAt = now;
        }
        else
        {
            membership.Status = ResidentMembershipStatuses.Rejected;
        }

        _unitOfWork.ResidentMemberships.Update(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(membership);
    }

    public async Task CancelInvitationAsync(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null)
        {
            throw new KeyNotFoundException("Organization not found.");
        }

        var membership = await _unitOfWork.ResidentMemberships.GetByIdAsync(membershipId, cancellationToken);
        if (membership is null || membership.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Invitation not found in this organization.");
        }

        if (membership.Status.Value != ResidentMembershipStatuses.Pending)
        {
            throw new InvalidOperationException("Only pending invitations can be cancelled.");
        }

        _unitOfWork.ResidentMemberships.Remove(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> CreateUniqueMembershipCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = $"RM{RandomNumberGenerator.GetInt32(100000, 999999)}";
            var exists = await _unitOfWork.ResidentMemberships.ExistsByMembershipCodeAsync(
                code,
                cancellationToken);

            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Could not generate a unique membership code.");
    }

    private static bool IsActiveStatus(string status)
    {
        return string.Equals(status, ResidentMembershipStatuses.Active, StringComparison.OrdinalIgnoreCase);
    }

    private static ResidentMembershipResponse ToResponse(ResidentMembershipEntity membership)
    {
        return new ResidentMembershipResponse
        {
            MembershipId = membership.Id,
            AccountId = membership.AccountId,
            ResidentId = membership.ResidentId,
            OrganizationId = membership.OrganizationId,
            MembershipCode = membership.MembershipCode,
            Status = membership.Status.Value,
            InvitedAt = membership.InvitedAt,
            RespondedAt = membership.RespondedAt,
            JoinedAt = membership.JoinedAt,
            ActivatedAt = membership.ActivatedAt,
            CreatedAt = membership.CreatedAt
        };
    }
}
