using NovaStay.Application.Common.Interfaces;
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
using DomainAccountAccessToken = NovaStay.Domain.Entities.AccountAccessTokenEntity;
using DatabaseAccountAccessToken = NovaStay.Infrastructure.Models.AccountAccessToken;
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
    public ResidentRepository(HostContext context, IDatabaseModelMapper<DomainResident, DatabaseResident> mapper)
        : base(context, mapper)
    {
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
    public RoomRepository(HostContext context, IDatabaseModelMapper<DomainRoom, DatabaseRoom> mapper)
        : base(context, mapper)
    {
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

internal sealed class AccountAccessTokenRepository : Repository<DomainAccountAccessToken, DatabaseAccountAccessToken>, IAccountAccessTokenRepository
{
    public AccountAccessTokenRepository(HostContext context, IDatabaseModelMapper<DomainAccountAccessToken, DatabaseAccountAccessToken> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class AccountRefreshTokenRepository : Repository<DomainAccountRefreshToken, DatabaseAccountRefreshToken>, IAccountRefreshTokenRepository
{
    public AccountRefreshTokenRepository(HostContext context, IDatabaseModelMapper<DomainAccountRefreshToken, DatabaseAccountRefreshToken> mapper)
        : base(context, mapper)
    {
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
