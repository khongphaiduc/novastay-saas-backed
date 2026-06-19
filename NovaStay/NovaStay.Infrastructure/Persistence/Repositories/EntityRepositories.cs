using NovaStay.Application.Common.Interfaces;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Mapping;
using DomainTenant = NovaStay.Domain.Entities.TenantEntity;
using DatabaseTenant = NovaStay.Infrastructure.Models.Tenant;
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
using DomainUserAccessToken = NovaStay.Domain.Entities.UserAccessTokenEntity;
using DatabaseUserAccessToken = NovaStay.Infrastructure.Models.UserAccessToken;
using DomainUserRefreshToken = NovaStay.Domain.Entities.UserRefreshTokenEntity;
using DatabaseUserRefreshToken = NovaStay.Infrastructure.Models.UserRefreshToken;
using DomainUser = NovaStay.Domain.Entities.UserEntity;
using DatabaseUser = NovaStay.Infrastructure.Models.User;
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

internal sealed class UserRepository : Repository<DomainUser, DatabaseUser>, IUserRepository
{
    public UserRepository(HostContext context, IDatabaseModelMapper<DomainUser, DatabaseUser> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class UserAccessTokenRepository : Repository<DomainUserAccessToken, DatabaseUserAccessToken>, IUserAccessTokenRepository
{
    public UserAccessTokenRepository(HostContext context, IDatabaseModelMapper<DomainUserAccessToken, DatabaseUserAccessToken> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class UserRefreshTokenRepository : Repository<DomainUserRefreshToken, DatabaseUserRefreshToken>, IUserRefreshTokenRepository
{
    public UserRefreshTokenRepository(HostContext context, IDatabaseModelMapper<DomainUserRefreshToken, DatabaseUserRefreshToken> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class TenantRepository : Repository<DomainTenant, DatabaseTenant>, ITenantRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainTenant, DatabaseTenant> _mapper;

    public TenantRepository(HostContext context, IDatabaseModelMapper<DomainTenant, DatabaseTenant> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainTenant?> GetByOwnerEmailAsync(string ownerEmail, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.OwnerEmail == ownerEmail, cancellationToken);

        return tenant is null ? null : _mapper.ToDomain(tenant);
    }
}
