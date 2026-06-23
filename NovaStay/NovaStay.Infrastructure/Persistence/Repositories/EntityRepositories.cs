using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.Common;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Mapping;
using DomainOrganization = NovaStay.Domain.Entities.OrganizationEntity;
using DatabaseOrganization = NovaStay.Infrastructure.Models.Organization;
using DomainAsset = NovaStay.Domain.Entities.AssetEntity;
using DatabaseAsset = NovaStay.Infrastructure.Models.Asset;
using DomainAssetAssignment = NovaStay.Domain.Entities.AssetAssignmentEntity;
using DatabaseAssetAssignment = NovaStay.Infrastructure.Models.AssetAssignment;
using DomainBooking = NovaStay.Domain.Entities.BookingEntity;
using DatabaseBooking = NovaStay.Infrastructure.Models.Booking;
using DomainBroker = NovaStay.Domain.Entities.BrokerEntity;
using DatabaseBroker = NovaStay.Infrastructure.Models.Broker;
using DomainContract = NovaStay.Domain.Entities.ContractEntity;
using DatabaseContract = NovaStay.Infrastructure.Models.Contract;
using DomainInvoice = NovaStay.Domain.Entities.InvoiceEntity;
using DatabaseInvoice = NovaStay.Infrastructure.Models.Invoice;
using DomainMaintenanceTicket = NovaStay.Domain.Entities.MaintenanceTicketEntity;
using DatabaseMaintenanceTicket = NovaStay.Infrastructure.Models.MaintenanceTicket;
using DomainPermission = NovaStay.Domain.Entities.PermissionEntity;
using DatabasePermission = NovaStay.Infrastructure.Models.Permission;
using DomainProperty = NovaStay.Domain.Entities.PropertyEntity;
using DatabaseProperty = NovaStay.Infrastructure.Models.Property;
using DomainResident = NovaStay.Domain.Entities.ResidentEntity;
using DatabaseResident = NovaStay.Infrastructure.Models.Resident;
using DomainResidentMembership = NovaStay.Domain.Entities.ResidentMembershipEntity;
using DatabaseResidentMembership = NovaStay.Infrastructure.Models.ResidentMembership;
using DomainRole = NovaStay.Domain.Entities.RoleEntity;
using DatabaseRole = NovaStay.Infrastructure.Models.Role;
using DomainRoom = NovaStay.Domain.Entities.RoomEntity;
using DatabaseRoom = NovaStay.Infrastructure.Models.Room;
using DomainRoomAvailability = NovaStay.Domain.Entities.RoomAvailabilityEntity;
using DatabaseRoomAvailability = NovaStay.Infrastructure.Models.RoomAvailability;
using DomainRoomImage = NovaStay.Domain.Entities.RoomImageEntity;
using DatabaseRoomImage = NovaStay.Infrastructure.Models.RoomImage;
using DomainSubscriptionPackage = NovaStay.Domain.Entities.SubscriptionPackageEntity;
using DatabaseSubscriptionPackage = NovaStay.Infrastructure.Models.SubscriptionPackage;
using DomainTechnician = NovaStay.Domain.Entities.TechnicianEntity;
using DatabaseTechnician = NovaStay.Infrastructure.Models.Technician;
using DomainAccount = NovaStay.Domain.Entities.AccountEntity;
using DatabaseAccount = NovaStay.Infrastructure.Models.Account;
using DomainAccountRefreshToken = NovaStay.Domain.Entities.AccountRefreshTokenEntity;
using DatabaseAccountRefreshToken = NovaStay.Infrastructure.Models.AccountRefreshToken;
using DomainStaffUser = NovaStay.Domain.Entities.StaffUserEntity;
using DatabaseStaffUser = NovaStay.Infrastructure.Models.StaffUser;
using Microsoft.EntityFrameworkCore;

namespace NovaStay.Infrastructure.Persistence.Repositories;

internal sealed class AssetRepository : Repository<DomainAsset, DatabaseAsset>, IAssetRepository
{
    public AssetRepository(HostContext context, IDatabaseModelMapper<DomainAsset, DatabaseAsset> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class AssetAssignmentRepository : Repository<DomainAssetAssignment, DatabaseAssetAssignment>, IAssetAssignmentRepository
{
    public AssetAssignmentRepository(HostContext context, IDatabaseModelMapper<DomainAssetAssignment, DatabaseAssetAssignment> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class BookingRepository : Repository<DomainBooking, DatabaseBooking>, IBookingRepository
{
    public BookingRepository(HostContext context, IDatabaseModelMapper<DomainBooking, DatabaseBooking> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class BrokerRepository : Repository<DomainBroker, DatabaseBroker>, IBrokerRepository
{
    public BrokerRepository(HostContext context, IDatabaseModelMapper<DomainBroker, DatabaseBroker> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class ContractRepository : Repository<DomainContract, DatabaseContract>, IContractRepository
{
    public ContractRepository(HostContext context, IDatabaseModelMapper<DomainContract, DatabaseContract> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class InvoiceRepository : Repository<DomainInvoice, DatabaseInvoice>, IInvoiceRepository
{
    public InvoiceRepository(HostContext context, IDatabaseModelMapper<DomainInvoice, DatabaseInvoice> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class MaintenanceTicketRepository : Repository<DomainMaintenanceTicket, DatabaseMaintenanceTicket>, IMaintenanceTicketRepository
{
    public MaintenanceTicketRepository(HostContext context, IDatabaseModelMapper<DomainMaintenanceTicket, DatabaseMaintenanceTicket> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class PermissionRepository : Repository<DomainPermission, DatabasePermission>, IPermissionRepository
{
    public PermissionRepository(HostContext context, IDatabaseModelMapper<DomainPermission, DatabasePermission> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class PropertyRepository : Repository<DomainProperty, DatabaseProperty>, IPropertyRepository
{
    public PropertyRepository(HostContext context, IDatabaseModelMapper<DomainProperty, DatabaseProperty> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class ResidentRepository : Repository<DomainResident, DatabaseResident>, IResidentRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainResident, DatabaseResident> _mapper;

    public ResidentRepository(HostContext context, IDatabaseModelMapper<DomainResident, DatabaseResident> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainResident?> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var resident = await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.AccountId == accountId,
                cancellationToken);

        return resident is null ? null : _mapper.ToDomain(resident);
    }

    public async Task<DomainResident?> GetByIdentityCardNumberAsync(
        string identityCardNumber,
        CancellationToken cancellationToken = default)
    {
        var resident = await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.IdentityCardNumber == identityCardNumber,
                cancellationToken);

        return resident is null ? null : _mapper.ToDomain(resident);
    }

    public async Task<IReadOnlyList<DomainResident>> SearchByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
    {
        var residents = await _context.Residents
            .AsNoTracking()
            .Where(resident => resident.Phone.Contains(phone))
            .OrderBy(resident => resident.FullName)
            .ToListAsync(cancellationToken);

        return residents.Select(_mapper.ToDomain).ToList();
    }
}

internal sealed class ResidentMembershipRepository : Repository<DomainResidentMembership, DatabaseResidentMembership>, IResidentMembershipRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainResidentMembership, DatabaseResidentMembership> _mapper;

    public ResidentMembershipRepository(HostContext context, IDatabaseModelMapper<DomainResidentMembership, DatabaseResidentMembership> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainResidentMembership?> GetActiveByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _context.ResidentMemberships
            .AsNoTracking()
            .Where(entity => entity.AccountId == accountId
                && (entity.Status == ResidentMembershipStatuses.Active
                    || entity.Status == ResidentMembershipStatuses.LegacyActive))
            .OrderByDescending(entity => entity.ActivatedAt ?? entity.JoinedAt ?? entity.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return membership is null ? null : _mapper.ToDomain(membership);
    }

    public async Task<DomainResidentMembership?> GetByResidentAndOrganizationAsync(
        Guid residentId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _context.ResidentMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.ResidentId == residentId && entity.OrganizationId == organizationId,
                cancellationToken);

        return membership is null ? null : _mapper.ToDomain(membership);
    }

    public async Task<DomainResidentMembership?> GetByIdForAccountAsync(
        Guid membershipId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _context.ResidentMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.Id == membershipId && entity.AccountId == accountId,
                cancellationToken);

        return membership is null ? null : _mapper.ToDomain(membership);
    }

    public Task<bool> ExistsByMembershipCodeAsync(
        string membershipCode,
        CancellationToken cancellationToken = default)
    {
        return _context.ResidentMemberships
            .AsNoTracking()
            .AnyAsync(
                entity => entity.MembershipCode == membershipCode,
                cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationResidentDto>> GetResidentsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        var query = _context.ResidentMemberships
            .AsNoTracking()
            .Where(membership => membership.OrganizationId == organizationId);

        if(normalizedStatus == null)
        {
            normalizedStatus = "ACTIVE";
        }

        if (normalizedStatus is not null)
        {
            query = query.Where(membership => membership.Status == normalizedStatus);
        }

        return await query
            .OrderBy(membership => membership.Status)
            .ThenBy(membership => membership.Resident.FullName)
            .Select(membership => new OrganizationResidentDto
            {
                MembershipId = membership.Id,
                OrganizationId = membership.OrganizationId,
                ResidentId = membership.ResidentId,
                AccountId = membership.AccountId,
                MembershipCode = membership.MembershipCode,
                MembershipStatus = membership.Status,
                InvitedAt = membership.InvitedAt,
                RespondedAt = membership.RespondedAt,
                ActivatedAt = membership.ActivatedAt,
                JoinedAt = membership.JoinedAt,
                FullName = membership.Resident.FullName,
                Phone = membership.Resident.Phone,
                Email = membership.Resident.Email,
                IdentityCardNumber = membership.Resident.IdentityCardNumber,
                ProfileImageUrl = membership.Resident.ProfileImageUrl,
                AccountIsActive = membership.Account.IsActive,
                MustSetPassword = membership.Account.MustSetPassword
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationResidentInvitationDto>> GetInvitationsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        var query = _context.ResidentMemberships
            .AsNoTracking()
            .Where(membership => membership.OrganizationId == organizationId);

        if (normalizedStatus is not null)
        {
            query = query.Where(membership => membership.Status == normalizedStatus);
        }

        return await query
            .OrderByDescending(membership => membership.InvitedAt ?? membership.CreatedAt)
            .ThenBy(membership => membership.Resident.FullName)
            .Select(membership => new OrganizationResidentInvitationDto
            {
                MembershipId = membership.Id,
                OrganizationId = membership.OrganizationId,
                ResidentId = membership.ResidentId,
                AccountId = membership.AccountId,
                MembershipCode = membership.MembershipCode,
                Status = membership.Status,
                InvitedAt = membership.InvitedAt,
                RespondedAt = membership.RespondedAt,
                JoinedAt = membership.JoinedAt,
                ActivatedAt = membership.ActivatedAt,
                ResidentName = membership.Resident.FullName,
                ResidentPhone = membership.Resident.Phone,
                ResidentEmail = "",
                IdentityCardNumber = ""
            })
            .ToListAsync(cancellationToken);
    }
}

internal sealed class RoleRepository : Repository<DomainRole, DatabaseRole>, IRoleRepository
{
    public RoleRepository(HostContext context, IDatabaseModelMapper<DomainRole, DatabaseRole> mapper)
        : base(context, mapper)
    {
    }
} 

internal sealed class RoomRepository : Repository<DomainRoom, DatabaseRoom>, IRoomRepository
{
    private readonly HostContext _context;

    public RoomRepository(HostContext context, IDatabaseModelMapper<DomainRoom, DatabaseRoom> mapper)
        : base(context, mapper)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsWithImagesAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.Trim().ToLower();
            query = query.Where(r => r.RoomNumber.ToLower().Contains(lower));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status.Trim());
        }

        return await query
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                BasePrice = r.BasePrice,
                Status = r.Status,
                MaxOccupants = r.MaxOccupants,
                AmenitiesJson = r.AmenitiesJson,
                RowVersion = r.RowVersion,
                CreatedAt = r.CreatedAt,
                IsDeleted = r.IsDeleted,
                Images = r.RoomImages
                    .OrderByDescending(i => i.IsCover)
                    .ThenBy(i => i.UploadedAt)
                    .Select(i => new RoomImageDto
                    {
                        Id = i.Id,
                        RoomId = i.RoomId,
                        ImageUrl = i.ImageUrl,
                        IsCover = i.IsCover,
                        UploadedAt = i.UploadedAt
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomDto?> GetRoomWithImagesAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(r => r.Id == roomId && !r.IsDeleted)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                BasePrice = r.BasePrice,
                Status = r.Status,
                MaxOccupants = r.MaxOccupants,
                AmenitiesJson = r.AmenitiesJson,
                RowVersion = r.RowVersion,
                CreatedAt = r.CreatedAt,
                IsDeleted = r.IsDeleted,
                Images = r.RoomImages
                    .OrderByDescending(i => i.IsCover)
                    .ThenBy(i => i.UploadedAt)
                    .Select(i => new RoomImageDto
                    {
                        Id = i.Id,
                        RoomId = i.RoomId,
                        ImageUrl = i.ImageUrl,
                        IsCover = i.IsCover,
                        UploadedAt = i.UploadedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

internal sealed class RoomAvailabilityRepository : Repository<DomainRoomAvailability, DatabaseRoomAvailability>, IRoomAvailabilityRepository
{
    public RoomAvailabilityRepository(HostContext context, IDatabaseModelMapper<DomainRoomAvailability, DatabaseRoomAvailability> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class RoomImageRepository : Repository<DomainRoomImage, DatabaseRoomImage>, IRoomImageRepository
{
    public RoomImageRepository(HostContext context, IDatabaseModelMapper<DomainRoomImage, DatabaseRoomImage> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class SubscriptionPackageRepository : Repository<DomainSubscriptionPackage, DatabaseSubscriptionPackage>, ISubscriptionPackageRepository
{
    public SubscriptionPackageRepository(HostContext context, IDatabaseModelMapper<DomainSubscriptionPackage, DatabaseSubscriptionPackage> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class TechnicianRepository : Repository<DomainTechnician, DatabaseTechnician>, ITechnicianRepository
{
    public TechnicianRepository(HostContext context, IDatabaseModelMapper<DomainTechnician, DatabaseTechnician> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class StaffUserRepository : Repository<DomainStaffUser, DatabaseStaffUser>, IStaffUserRepository
{
    public StaffUserRepository(HostContext context, IDatabaseModelMapper<DomainStaffUser, DatabaseStaffUser> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class AccountRepository : Repository<DomainAccount, DatabaseAccount>, IAccountRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainAccount, DatabaseAccount> _mapper;

    public AccountRepository(HostContext context, IDatabaseModelMapper<DomainAccount, DatabaseAccount> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainAccount?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Phone == phone, cancellationToken);

        return account is null ? null : _mapper.ToDomain(account);
    }

    public async Task<DomainAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Email == email, cancellationToken);

        return account is null ? null : _mapper.ToDomain(account);
    }
}

internal sealed class AccountRefreshTokenRepository : Repository<DomainAccountRefreshToken, DatabaseAccountRefreshToken>, IAccountRefreshTokenRepository
{
    private readonly HostContext _context;

    public AccountRefreshTokenRepository(HostContext context, IDatabaseModelMapper<DomainAccountRefreshToken, DatabaseAccountRefreshToken> mapper)
        : base(context, mapper)
    {
        _context = context;
    }

    public async Task<bool> RevokeByTokenHashAsync(
        string tokenHash,
        DateTime revokedAt,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.AccountRefreshTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash && token.RevokedAt == null,
                cancellationToken);

        if (refreshToken is null)
        {
            return false;
        }

        refreshToken.RevokedAt = revokedAt;
        refreshToken.RevokedByIp = revokedByIp;

        return true;
    }

    public async Task<int> RevokeActiveByAccountIdAsync(
        Guid accountId,
        DateTime revokedAt,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        var refreshTokens = await _context.AccountRefreshTokens
            .Where(token => token.AccountId == accountId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = revokedAt;
            refreshToken.RevokedByIp = revokedByIp;
        }

        return refreshTokens.Count;
    }
}

internal sealed class OrganizationRepository : Repository<DomainOrganization, DatabaseOrganization>, IOrganizationRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainOrganization, DatabaseOrganization> _mapper;

    public OrganizationRepository(HostContext context, IDatabaseModelMapper<DomainOrganization, DatabaseOrganization> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainOrganization?> GetByOwnerEmailAsync(string ownerEmail, CancellationToken cancellationToken = default)
    {
        var Organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.OwnerEmail == ownerEmail, cancellationToken);

        return Organization is null ? null : _mapper.ToDomain(Organization);
    }
}
