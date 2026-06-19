using AutoMapper;
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
using DomainTenant = NovaStay.Domain.Entities.TenantEntity;
using DatabaseTenant = NovaStay.Infrastructure.Models.Tenant;
using DomainUserAccessToken = NovaStay.Domain.Entities.UserAccessTokenEntity;
using DatabaseUserAccessToken = NovaStay.Infrastructure.Models.UserAccessToken;
using DomainUserRefreshToken = NovaStay.Domain.Entities.UserRefreshTokenEntity;
using DatabaseUserRefreshToken = NovaStay.Infrastructure.Models.UserRefreshToken;
using DomainUser = NovaStay.Domain.Entities.UserEntity;
using DatabaseUser = NovaStay.Infrastructure.Models.User;

namespace NovaStay.Infrastructure.Persistence.Mapping;

public sealed class DatabaseModelMappingProfile : Profile
{
    public DatabaseModelMappingProfile()
    {
        CreateMap<DatabaseAsset, DomainAsset>().ReverseMap();
        CreateMap<DatabaseAssetAssignment, DomainAssetAssignment>().ReverseMap();
        CreateMap<DatabaseBooking, DomainBooking>().ReverseMap();
        CreateMap<DatabaseBroker, DomainBroker>().ReverseMap();
        CreateMap<DatabaseContract, DomainContract>().ReverseMap();
        CreateMap<DatabaseInvoice, DomainInvoice>().ReverseMap();
        CreateMap<DatabaseMaintenanceTicket, DomainMaintenanceTicket>().ReverseMap();
        CreateMap<DatabasePermission, DomainPermission>().ReverseMap();
        CreateMap<DatabaseProperty, DomainProperty>().ReverseMap();
        CreateMap<DatabaseResident, DomainResident>().ReverseMap();
        CreateMap<DatabaseRole, DomainRole>().ReverseMap();
        CreateMap<DatabaseRoom, DomainRoom>().ReverseMap();
        CreateMap<DatabaseRoomAvailability, DomainRoomAvailability>().ReverseMap();
        CreateMap<DatabaseRoomImage, DomainRoomImage>().ReverseMap();
        CreateMap<DatabaseSubscriptionPackage, DomainSubscriptionPackage>().ReverseMap();
        CreateMap<DatabaseTechnician, DomainTechnician>().ReverseMap();
        CreateMap<DatabaseTenant, DomainTenant>().ReverseMap();
        CreateMap<DatabaseUserAccessToken, DomainUserAccessToken>().ReverseMap();
        CreateMap<DatabaseUserRefreshToken, DomainUserRefreshToken>().ReverseMap();
        CreateMap<DatabaseUser, DomainUser>().ReverseMap();
    }
}
