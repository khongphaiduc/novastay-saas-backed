namespace NovaStay.Application.Common.Interfaces;

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
    IPropertyServiceRepository PropertyServices { get; }
    IResidentRepository Residents { get; }
    IResidentMembershipRepository ResidentMemberships { get; }
    IRoleRepository Roles { get; }
    IRoomRepository Rooms { get; }
    IRoomAvailabilityRepository RoomAvailabilities { get; }
    IRoomImageRepository RoomImages { get; }
    ISubscriptionPackageRepository SubscriptionPackages { get; }
    ITechnicianRepository Technicians { get; }
    IAccountRepository Accounts { get; }
    IOrganizationRepository Organizations { get; }
    IAccountRefreshTokenRepository AccountRefreshTokens { get; }
    IStaffUserRepository StaffUsers { get; }
    IIncomeReceiptRepository IncomeReceipts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
