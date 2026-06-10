using Host.Application.Common.Interfaces;
using Host.Infrastructure.ContextDB;

namespace Host.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly HostContext _context;

    public UnitOfWork(
        HostContext context,
        IAssetRepository assets,
        IAssetAssignmentRepository assetAssignments,
        IBedRepository beds,
        IBookingRepository bookings,
        IBrokerRepository brokers,
        IContractRepository contracts,
        IInvoiceRepository invoices,
        IListingRepository listings,
        IListingImageRepository listingImages,
        IMaintenanceTicketRepository maintenanceTickets,
        IPermissionRepository permissions,
        IPropertyRepository properties,
        IResidentRepository residents,
        IRoleRepository roles,
        IRoomRepository rooms,
        IRoomAvailabilityRepository roomAvailabilities,
        IRoomImageRepository roomImages,
        ISubscriptionPackageRepository subscriptionPackages,
        ITechnicianRepository technicians,
        ITenantRepository tenants,
        IUserRepository users)
    {
        _context = context;
        Assets = assets;
        AssetAssignments = assetAssignments;
        Beds = beds;
        Bookings = bookings;
        Brokers = brokers;
        Contracts = contracts;
        Invoices = invoices;
        Listings = listings;
        ListingImages = listingImages;
        MaintenanceTickets = maintenanceTickets;
        Permissions = permissions;
        Properties = properties;
        Residents = residents;
        Roles = roles;
        Rooms = rooms;
        RoomAvailabilities = roomAvailabilities;
        RoomImages = roomImages;
        SubscriptionPackages = subscriptionPackages;
        Technicians = technicians;
        Tenants = tenants;
        Users = users;
    }

    public IAssetRepository Assets { get; }

    public IAssetAssignmentRepository AssetAssignments { get; }

    public IBedRepository Beds { get; }

    public IBookingRepository Bookings { get; }

    public IBrokerRepository Brokers { get; }

    public IContractRepository Contracts { get; }

    public IInvoiceRepository Invoices { get; }

    public IListingRepository Listings { get; }

    public IListingImageRepository ListingImages { get; }

    public IMaintenanceTicketRepository MaintenanceTickets { get; }

    public IPermissionRepository Permissions { get; }

    public IPropertyRepository Properties { get; }

    public IResidentRepository Residents { get; }

    public IRoleRepository Roles { get; }

    public IRoomRepository Rooms { get; }

    public IRoomAvailabilityRepository RoomAvailabilities { get; }

    public IRoomImageRepository RoomImages { get; }

    public ISubscriptionPackageRepository SubscriptionPackages { get; }

    public ITechnicianRepository Technicians { get; }

    public ITenantRepository Tenants { get; }

    public IUserRepository Users { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
