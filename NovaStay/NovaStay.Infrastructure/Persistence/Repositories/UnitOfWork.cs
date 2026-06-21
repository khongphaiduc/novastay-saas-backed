using NovaStay.Application.Common.Interfaces;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly HostContext _context;

    public UnitOfWork(
        HostContext context,
        IAssetRepository assets,
        IAssetAssignmentRepository assetAssignments,
        IBookingRepository bookings,
        IBrokerRepository brokers,
        IContractRepository contracts,
        IInvoiceRepository invoices,
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
        IAccountRepository accounts,
        IOrganizationRepository organizations,
        IAccountAccessTokenRepository accountAccessTokens,
        IAccountRefreshTokenRepository accountRefreshTokens,
        IStaffUserRepository staffUsers)
    {
        _context = context;
        Assets = assets;
        AssetAssignments = assetAssignments;
        Bookings = bookings;
        Brokers = brokers;
        Contracts = contracts;
        Invoices = invoices;
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
        Accounts = accounts;
        Organizations = organizations;
        AccountAccessTokens = accountAccessTokens;
        AccountRefreshTokens = accountRefreshTokens;
        StaffUsers = staffUsers;
    }

    public IAssetRepository Assets { get; }

    public IAssetAssignmentRepository AssetAssignments { get; }

    public IBookingRepository Bookings { get; }

    public IBrokerRepository Brokers { get; }

    public IContractRepository Contracts { get; }

    public IInvoiceRepository Invoices { get; }

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

    public IAccountRepository Accounts { get; }

    public IOrganizationRepository Organizations { get; }

    public IAccountAccessTokenRepository AccountAccessTokens { get; }

    public IAccountRefreshTokenRepository AccountRefreshTokens { get; }

    public IStaffUserRepository StaffUsers { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
