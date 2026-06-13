using AutoMapper;
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
using DomainListing = Host.Domain.Entities.ListingEntity;
using DatabaseListing = Host.Infrastructure.Models.Listing;
using DomainListingImage = Host.Domain.Entities.ListingImageEntity;
using DatabaseListingImage = Host.Infrastructure.Models.ListingImage;
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
using DomainTenant = Host.Domain.Entities.TenantEntity;
using DatabaseTenant = Host.Infrastructure.Models.Tenant;
using DomainUserAccessToken = Host.Domain.Entities.UserAccessTokenEntity;
using DatabaseUserAccessToken = Host.Infrastructure.Models.UserAccessToken;
using DomainUserRefreshToken = Host.Domain.Entities.UserRefreshTokenEntity;
using DatabaseUserRefreshToken = Host.Infrastructure.Models.UserRefreshToken;
using DomainUser = Host.Domain.Entities.UserEntity;
using DatabaseUser = Host.Infrastructure.Models.User;

namespace Host.Infrastructure.Persistence.Mapping;

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
        CreateMap<DatabaseListing, DomainListing>().ReverseMap();
        CreateMap<DatabaseListingImage, DomainListingImage>().ReverseMap();
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
