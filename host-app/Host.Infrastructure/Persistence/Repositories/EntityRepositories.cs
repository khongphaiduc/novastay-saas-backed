using Host.Application.Common.Interfaces;
using Host.Infrastructure.ContextDB;
using Host.Infrastructure.Persistence.Mapping;
using DomainTenant = Host.Domain.Entities.Tenant;
using DatabaseTenant = Host.Infrastructure.Models.Tenant;
using DomainAsset = Host.Domain.Entities.Asset;
using DatabaseAsset = Host.Infrastructure.Models.Asset;
using DomainAssetAssignment = Host.Domain.Entities.AssetAssignment;
using DatabaseAssetAssignment = Host.Infrastructure.Models.AssetAssignment;
using DomainBed = Host.Domain.Entities.Bed;
using DatabaseBed = Host.Infrastructure.Models.Bed;
using DomainBooking = Host.Domain.Entities.Booking;
using DatabaseBooking = Host.Infrastructure.Models.Booking;
using DomainBroker = Host.Domain.Entities.Broker;
using DatabaseBroker = Host.Infrastructure.Models.Broker;
using DomainContract = Host.Domain.Entities.Contract;
using DatabaseContract = Host.Infrastructure.Models.Contract;
using DomainInvoice = Host.Domain.Entities.Invoice;
using DatabaseInvoice = Host.Infrastructure.Models.Invoice;
using DomainListing = Host.Domain.Entities.Listing;
using DatabaseListing = Host.Infrastructure.Models.Listing;
using DomainListingImage = Host.Domain.Entities.ListingImage;
using DatabaseListingImage = Host.Infrastructure.Models.ListingImage;
using DomainMaintenanceTicket = Host.Domain.Entities.MaintenanceTicket;
using DatabaseMaintenanceTicket = Host.Infrastructure.Models.MaintenanceTicket;
using DomainPermission = Host.Domain.Entities.Permission;
using DatabasePermission = Host.Infrastructure.Models.Permission;
using DomainProperty = Host.Domain.Entities.Property;
using DatabaseProperty = Host.Infrastructure.Models.Property;
using DomainResident = Host.Domain.Entities.Resident;
using DatabaseResident = Host.Infrastructure.Models.Resident;
using DomainRole = Host.Domain.Entities.Role;
using DatabaseRole = Host.Infrastructure.Models.Role;
using DomainRoom = Host.Domain.Entities.Room;
using DatabaseRoom = Host.Infrastructure.Models.Room;
using DomainRoomAvailability = Host.Domain.Entities.RoomAvailability;
using DatabaseRoomAvailability = Host.Infrastructure.Models.RoomAvailability;
using DomainRoomImage = Host.Domain.Entities.RoomImage;
using DatabaseRoomImage = Host.Infrastructure.Models.RoomImage;
using DomainSubscriptionPackage = Host.Domain.Entities.SubscriptionPackage;
using DatabaseSubscriptionPackage = Host.Infrastructure.Models.SubscriptionPackage;
using DomainTechnician = Host.Domain.Entities.Technician;
using DatabaseTechnician = Host.Infrastructure.Models.Technician;
using DomainUser = Host.Domain.Entities.User;
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

internal sealed class BedRepository : Repository<DomainBed, DatabaseBed>, IBedRepository
{
    public BedRepository(HostContext context, IDatabaseModelMapper<DomainBed, DatabaseBed> mapper)
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

internal sealed class ListingRepository : Repository<DomainListing, DatabaseListing>, IListingRepository
{
    public ListingRepository(HostContext context, IDatabaseModelMapper<DomainListing, DatabaseListing> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class ListingImageRepository : Repository<DomainListingImage, DatabaseListingImage>, IListingImageRepository
{
    public ListingImageRepository(HostContext context, IDatabaseModelMapper<DomainListingImage, DatabaseListingImage> mapper)
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