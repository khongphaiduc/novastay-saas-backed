using Host.Application.Common.Interfaces;
using Host.Infrastructure.ContextDB;
using Host.Infrastructure.Persistence.Mapping;
using DomainTenant = Host.Domain.Entities.TenantEntity;
using DatabaseTenant = Host.Infrastructure.Models.Tenant;
using DomainAsset = Host.Domain.Entities.AssetEntity;
using DatabaseAsset = Host.Infrastructure.Models.Asset;
using DomainAssetAssignment = Host.Domain.Entities.AssetAssignmentEntity;
using DatabaseAssetAssignment = Host.Infrastructure.Models.AssetAssignment;
using DomainBooking = Host.Domain.Entities.BookingEntity;
using DatabaseBooking = Host.Infrastructure.Models.Booking;
using DomainBroker = Host.Domain.Entities.BrokerEntity;
using DatabaseBroker = Host.Infrastructure.Models.Broker;
using DomainContract = Host.Domain.Entities.ContractEntity;
using DatabaseContract = Host.Infrastructure.Models.Contract;
using DomainInvoice = Host.Domain.Entities.InvoiceEntity;
using DatabaseInvoice = Host.Infrastructure.Models.Invoice;
using DomainMaintenanceTicket = Host.Domain.Entities.MaintenanceTicketEntity;
using DatabaseMaintenanceTicket = Host.Infrastructure.Models.MaintenanceTicket;
using DomainPermission = Host.Domain.Entities.PermissionEntity;
using DatabasePermission = Host.Infrastructure.Models.Permission;
using DomainProperty = Host.Domain.Entities.PropertyEntity;
using DatabaseProperty = Host.Infrastructure.Models.Property;
using DomainResident = Host.Domain.Entities.ResidentEntity;
using DatabaseResident = Host.Infrastructure.Models.Resident;
using DomainRole = Host.Domain.Entities.RoleEntity;
using DatabaseRole = Host.Infrastructure.Models.Role;
using DomainRoom = Host.Domain.Entities.RoomEntity;
using DatabaseRoom = Host.Infrastructure.Models.Room;
using DomainRoomAvailability = Host.Domain.Entities.RoomAvailabilityEntity;
using DatabaseRoomAvailability = Host.Infrastructure.Models.RoomAvailability;
using DomainRoomImage = Host.Domain.Entities.RoomImageEntity;
using DatabaseRoomImage = Host.Infrastructure.Models.RoomImage;
using DomainSubscriptionPackage = Host.Domain.Entities.SubscriptionPackageEntity;
using DatabaseSubscriptionPackage = Host.Infrastructure.Models.SubscriptionPackage;
using DomainTechnician = Host.Domain.Entities.TechnicianEntity;
using DatabaseTechnician = Host.Infrastructure.Models.Technician;
using DomainUserAccessToken = Host.Domain.Entities.UserAccessTokenEntity;
using DatabaseUserAccessToken = Host.Infrastructure.Models.UserAccessToken;
using DomainUserRefreshToken = Host.Domain.Entities.UserRefreshTokenEntity;
using DatabaseUserRefreshToken = Host.Infrastructure.Models.UserRefreshToken;
using DomainUser = Host.Domain.Entities.UserEntity;
using DatabaseUser = Host.Infrastructure.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Host.Infrastructure.Persistence.Repositories;

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
