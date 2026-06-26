using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Auth;
using NovaStay.Infrastructure.Persistence.Mapping;
using NovaStay.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStay.Infrastructure.ServicesImple;

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
        services.AddScoped<IResidentMembershipRepository, ResidentMembershipRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();
        services.AddScoped<IRoomImageRepository, RoomImageRepository>();
        services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IAccountRefreshTokenRepository, AccountRefreshTokenRepository>();
        services.AddScoped<IStaffUserRepository, StaffUserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChangePasswordService, ChangePasswordService>();
        services.AddScoped<IOrganizationResidentService, OrganizationResidentService>();
        services.AddScoped<IResidentService, ResidentService>();
        services.AddScoped<IResidentInvitationService, ResidentInvitationService>();
        services.AddScoped<IResidentAuthService, ResidentAuthService>();
        services.AddScoped<ILogoutService, LogoutService>();
        services.AddScoped<INotifications, Email>();

        // Room Management
        services.AddSingleton<IMinioStorageService, MinioStorageService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IPropertyService, PropertyService>();

        // Asset Management
        services.AddScoped<IAssetService, AssetService>();

        return services;
    }
}
