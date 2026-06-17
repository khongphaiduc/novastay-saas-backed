namespace Host.Application.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAssetRepository Assets { get; }
    IAssetAssignmentRepository AssetAssignments { get; }
    IBookingRepository Bookings { get; }
    IBrokerRepository Brokers { get; }
    IContractRepository Contracts { get; }
    IInvoiceRepository Invoices { get; }
    IMaintenanceTicketRepository MaintenanceTickets { get; }
    IPermissionRepository Permissions { get; }
    IPropertyRepository Properties { get; }
    IResidentRepository Residents { get; }
    IRoleRepository Roles { get; }
    IRoomRepository Rooms { get; }
    IRoomAvailabilityRepository RoomAvailabilities { get; }
    IRoomImageRepository RoomImages { get; }
    ISubscriptionPackageRepository SubscriptionPackages { get; }
    ITechnicianRepository Technicians { get; }
    ITenantRepository Tenants { get; }
    IUserAccessTokenRepository UserAccessTokens { get; }
    IUserRefreshTokenRepository UserRefreshTokens { get; }
    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
