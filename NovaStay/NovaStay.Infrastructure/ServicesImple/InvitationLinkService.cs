using Microsoft.EntityFrameworkCore;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Infrastructure.ContextDB;
using System.Security.Cryptography;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class InvitationLinkService : IInvitationLinkService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly HostContext _context;

    public InvitationLinkService(IUnitOfWork unitOfWork, HostContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<InvitationLinkDto> CreateAsync(
        Guid organizationId,
        CreateInvitationLinkRequest request,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        // Kiểm tra organization tồn tại
        var org = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tổ chức.");

        // Sinh token ngẫu nhiên 32 bytes (64 ký tự hex)
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var now = DateTime.UtcNow;

        var link = new InvitationLinkEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = request.PropertyId,
            RoomId = request.RoomId,
            Token = token,
            ExpiresAt = now.AddDays(request.ExpiresInDays),
            MaxUses = request.MaxUses,
            UsedCount = 0,
            IsActive = true,
            Note = request.Note,
            CreatedAt = now
        };

        await _unitOfWork.InvitationLinks.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDto(link, baseUrl, cancellationToken);
    }

    public async Task<IReadOnlyList<InvitationLinkDto>> GetByOrganizationAsync(
        Guid organizationId,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        var links = await _unitOfWork.InvitationLinks.GetByOrganizationAsync(organizationId, cancellationToken);
        var result = new List<InvitationLinkDto>();
        foreach (var link in links)
        {
            result.Add(await MapToDto(link, baseUrl, cancellationToken));
        }
        return result;
    }

    public async Task DeactivateAsync(
        Guid organizationId,
        Guid linkId,
        CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.InvitationLinks.GetByIdAsync(linkId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy link mời.");

        if (link.OrganizationId != organizationId)
            throw new InvalidOperationException("Link mời này không thuộc về tổ chức của bạn.");

        link.IsActive = false;
        _unitOfWork.InvitationLinks.Update(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<InvitationLinkPreviewDto> GetPreviewAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.InvitationLinks.GetByTokenAsync(token, cancellationToken);

        if (link is null)
        {
            return new InvitationLinkPreviewDto
            {
                Token = token,
                IsValid = false,
                InvalidReason = "Link mời không tồn tại."
            };
        }

        var org = await _unitOfWork.Organizations.GetByIdAsync(link.OrganizationId, cancellationToken);
        string? propertyName = null;
        string? roomName = null;

        if (link.PropertyId.HasValue)
        {
            var prop = await _context.Properties.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == link.PropertyId.Value, cancellationToken);
            propertyName = prop?.PropertyName;
        }

        if (link.RoomId.HasValue)
        {
            var room = await _context.Rooms.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == link.RoomId.Value, cancellationToken);
            roomName = room?.RoomNumber;
        }

        // Kiểm tra tính hợp lệ
        if (!link.IsActive)
            return new InvitationLinkPreviewDto { Token = token, BusinessName = org?.BusinessName ?? "", IsValid = false, InvalidReason = "Link mời đã bị vô hiệu hoá." };

        if (link.ExpiresAt < DateTime.UtcNow)
            return new InvitationLinkPreviewDto { Token = token, BusinessName = org?.BusinessName ?? "", IsValid = false, InvalidReason = "Link mời đã hết hạn." };

        if (link.MaxUses > 0 && link.UsedCount >= link.MaxUses)
            return new InvitationLinkPreviewDto { Token = token, BusinessName = org?.BusinessName ?? "", IsValid = false, InvalidReason = "Link mời đã đạt giới hạn số lượt sử dụng." };

        return new InvitationLinkPreviewDto
        {
            Token = token,
            BusinessName = org?.BusinessName ?? "",
            PropertyName = propertyName,
            RoomName = roomName,
            ExpiresAt = link.ExpiresAt,
            IsValid = true
        };
    }

    public async Task<ResidentMembershipResponse> AcceptAsync(
        Guid accountId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.InvitationLinks.GetByTokenAsync(token, cancellationToken)
            ?? throw new KeyNotFoundException("Link mời không tồn tại.");

        if (!link.IsActive)
            throw new InvalidOperationException("Link mời đã bị vô hiệu hoá.");

        if (link.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Link mời đã hết hạn.");

        if (link.MaxUses > 0 && link.UsedCount >= link.MaxUses)
            throw new InvalidOperationException("Link mời đã đạt giới hạn số lượt sử dụng.");

        // Lấy thông tin Resident của accountId
        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy thông tin người dùng.");

        // Kiểm tra đã là thành viên chưa
        var existing = await _unitOfWork.ResidentMemberships
            .GetByResidentAndOrganizationAsync(resident.Id, link.OrganizationId, cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException("Bạn đã là thành viên của tổ chức này.");

        var now = DateTime.UtcNow;
        var membershipCode = $"MBR-{Guid.NewGuid():N}".ToUpperInvariant()[..16];

        var membership = new ResidentMembershipEntity
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            ResidentId = resident.Id,
            OrganizationId = link.OrganizationId,
            MembershipCode = membershipCode,
            Status = "PENDING_APPROVAL",
            InvitedAt = now,
            CreatedAt = now
        };

        await _unitOfWork.ResidentMemberships.AddAsync(membership, cancellationToken);

        // Tăng số lượt đã dùng
        link.UsedCount++;
        _unitOfWork.InvitationLinks.Update(link);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResidentMembershipResponse
        {
            MembershipId = membership.Id,
            AccountId = accountId,
            ResidentId = resident.Id,
            OrganizationId = link.OrganizationId,
            MembershipCode = membershipCode,
            Status = "PENDING_APPROVAL",
            InvitedAt = now,
            CreatedAt = now
        };
    }

    public async Task<IReadOnlyList<PendingResidentApprovalDto>> GetPendingApprovalsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var list = await _context.ResidentMemberships
            .AsNoTracking()
            .Include(m => m.Resident)
            .Include(m => m.Account)
            .Where(m => m.OrganizationId == organizationId && m.Status == "PENDING_APPROVAL")
            .OrderByDescending(m => m.CreatedAt ?? m.InvitedAt)
            .ToListAsync(cancellationToken);

        return list.Select(m => new PendingResidentApprovalDto
        {
            MembershipId = m.Id,
            ResidentId = m.ResidentId,
            AccountId = m.AccountId,
            MembershipCode = m.MembershipCode,
            FullName = m.Resident != null && !string.IsNullOrWhiteSpace(m.Resident.FullName)
                ? m.Resident.FullName
                : (m.Account != null && !string.IsNullOrWhiteSpace(m.Account.CustomerName) ? m.Account.CustomerName : "Chưa rõ"),
            Email = m.Resident?.Email ?? m.Account?.Email,
            Phone = !string.IsNullOrEmpty(m.Resident?.Phone) ? m.Resident.Phone : m.Account?.Phone,
            IdentityCardNumber = m.Resident?.IdentityCardNumber,
            Status = m.Status,
            RequestedAt = m.CreatedAt ?? m.InvitedAt
        }).ToList();
    }

    public async Task<ResidentMembershipResponse> ApprovePendingResidentAsync(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var dbMembership = await _context.ResidentMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu gia nhập.");

        if (dbMembership.OrganizationId != organizationId)
        {
            throw new InvalidOperationException("Yêu cầu này không thuộc về tổ chức của bạn.");
        }

        if (dbMembership.Status != "PENDING_APPROVAL")
        {
            throw new InvalidOperationException($"Yêu cầu này đang ở trạng thái '{dbMembership.Status}', không thể duyệt.");
        }

        var now = DateTime.UtcNow;
        dbMembership.Status = "ACTIVE";
        dbMembership.JoinedAt = now;
        dbMembership.ActivatedAt = now;
        dbMembership.RespondedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return new ResidentMembershipResponse
        {
            MembershipId = dbMembership.Id,
            AccountId = dbMembership.AccountId,
            ResidentId = dbMembership.ResidentId,
            OrganizationId = dbMembership.OrganizationId,
            MembershipCode = dbMembership.MembershipCode,
            Status = dbMembership.Status,
            InvitedAt = dbMembership.InvitedAt,
            CreatedAt = dbMembership.CreatedAt
        };
    }

    public async Task<ResidentMembershipResponse> RejectPendingResidentAsync(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var dbMembership = await _context.ResidentMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu gia nhập.");

        if (dbMembership.OrganizationId != organizationId)
        {
            throw new InvalidOperationException("Yêu cầu này không thuộc về tổ chức của bạn.");
        }

        if (dbMembership.Status != "PENDING_APPROVAL")
        {
            throw new InvalidOperationException($"Yêu cầu này đang ở trạng thái '{dbMembership.Status}', không thể từ chối.");
        }

        var now = DateTime.UtcNow;
        dbMembership.Status = "REJECTED";
        dbMembership.RespondedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return new ResidentMembershipResponse
        {
            MembershipId = dbMembership.Id,
            AccountId = dbMembership.AccountId,
            ResidentId = dbMembership.ResidentId,
            OrganizationId = dbMembership.OrganizationId,
            MembershipCode = dbMembership.MembershipCode,
            Status = dbMembership.Status,
            InvitedAt = dbMembership.InvitedAt,
            CreatedAt = dbMembership.CreatedAt
        };
    }


    // ── Private helpers ────────────────────────────────────────────────────────
    private async Task<InvitationLinkDto> MapToDto(InvitationLinkEntity link, string baseUrl, CancellationToken ct)
    {
        string? propertyName = null;
        string? roomName = null;

        if (link.PropertyId.HasValue)
        {
            var prop = await _context.Properties.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == link.PropertyId.Value, ct);
            propertyName = prop?.PropertyName;
        }

        if (link.RoomId.HasValue)
        {
            var room = await _context.Rooms.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == link.RoomId.Value, ct);
            roomName = room?.RoomNumber;
        }

        return new InvitationLinkDto
        {
            Id = link.Id,
            OrganizationId = link.OrganizationId,
            PropertyId = link.PropertyId,
            RoomId = link.RoomId,
            Token = link.Token,
            InviteUrl = $"{baseUrl.TrimEnd('/')}/invitation/{link.Token}",
            ExpiresAt = link.ExpiresAt,
            MaxUses = link.MaxUses,
            UsedCount = link.UsedCount,
            IsActive = link.IsActive,
            Note = link.Note,
            CreatedAt = link.CreatedAt,
            PropertyName = propertyName,
            RoomName = roomName
        };
    }
}
