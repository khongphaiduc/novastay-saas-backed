using Host.Application.Common.Interfaces;
using Host.Infrastructure.ContextDB;
using Host.Infrastructure.Persistence.Mapping;
using Host.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
using DomainTenant = Host.Domain.Entities.Tenant;
using DatabaseTenant = Host.Infrastructure.Models.Tenant;
using DomainUser = Host.Domain.Entities.User;
using DatabaseUser = Host.Infrastructure.Models.User;

namespace Host.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<HostContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IDatabaseModelMapper<DomainAsset, DatabaseAsset>, AssetMapper>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainAssetAssignment, DatabaseAssetAssignment>, AssetAssignmentMapper>();
        services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainBed, DatabaseBed>, BedMapper>();
        services.AddScoped<IBedRepository, BedRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainBooking, DatabaseBooking>, BookingMapper>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainBroker, DatabaseBroker>, BrokerMapper>();
        services.AddScoped<IBrokerRepository, BrokerRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainContract, DatabaseContract>, ContractMapper>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainInvoice, DatabaseInvoice>, InvoiceMapper>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainListing, DatabaseListing>, ListingMapper>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainListingImage, DatabaseListingImage>, ListingImageMapper>();
        services.AddScoped<IListingImageRepository, ListingImageRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainMaintenanceTicket, DatabaseMaintenanceTicket>, MaintenanceTicketMapper>();
        services.AddScoped<IMaintenanceTicketRepository, MaintenanceTicketRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainPermission, DatabasePermission>, PermissionMapper>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainProperty, DatabaseProperty>, PropertyMapper>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainResident, DatabaseResident>, ResidentMapper>();
        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainRole, DatabaseRole>, RoleMapper>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainRoom, DatabaseRoom>, RoomMapper>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainRoomAvailability, DatabaseRoomAvailability>, RoomAvailabilityMapper>();
        services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainRoomImage, DatabaseRoomImage>, RoomImageMapper>();
        services.AddScoped<IRoomImageRepository, RoomImageRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainSubscriptionPackage, DatabaseSubscriptionPackage>, SubscriptionPackageMapper>();
        services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainTechnician, DatabaseTechnician>, TechnicianMapper>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainTenant, DatabaseTenant>, TenantMapper>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IDatabaseModelMapper<DomainUser, DatabaseUser>, UserMapper>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}