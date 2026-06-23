using NovaStay.Application.Common;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
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

    public async Task<IReadOnlyList<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return null;
        }

        return await _residentMembershipRepository.GetResidentsByOrganizationAsync(
            organizationId,
            status,
            cancellationToken);
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
        if (existingMembership is not null)
        {
            throw new InvalidOperationException("Resident already has a membership with this organization.");
        }

        var now = DateTime.UtcNow;
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
