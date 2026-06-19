using NovaStay.Application.Common.Interfaces;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Mapping;
using NovaStay.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaStay.Infrastructure.Persistence.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["SQLString"];

        services.AddDbContext<HostContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IDatabaseModelMapper<,>), typeof(DatabaseModelMapper<,>));

        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBrokerRepository, BrokerRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IMaintenanceTicketRepository, MaintenanceTicketRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();
        services.AddScoped<IRoomImageRepository, RoomImageRepository>();
        services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserAccessTokenRepository, UserAccessTokenRepository>();
        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
