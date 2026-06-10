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

namespace Host.Infrastructure.Persistence.Mapping;

internal sealed class AssetMapper : IDatabaseModelMapper<DomainAsset, DatabaseAsset>
{
    public DomainAsset ToDomain(DatabaseAsset databaseModel)
    {
        return new DomainAsset
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            AssetName = databaseModel.AssetName,
            Brand = databaseModel.Brand,
            Model = databaseModel.Model,
            AssetCode = databaseModel.AssetCode,
            PurchaseDate = databaseModel.PurchaseDate,
            WarrantyExpiryDate = databaseModel.WarrantyExpiryDate,
            BaseValue = databaseModel.BaseValue,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseAsset ToDatabase(DomainAsset domainEntity)
    {
        return new DatabaseAsset
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            AssetName = domainEntity.AssetName,
            Brand = domainEntity.Brand,
            Model = domainEntity.Model,
            AssetCode = domainEntity.AssetCode,
            PurchaseDate = domainEntity.PurchaseDate,
            WarrantyExpiryDate = domainEntity.WarrantyExpiryDate,
            BaseValue = domainEntity.BaseValue,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class AssetAssignmentMapper : IDatabaseModelMapper<DomainAssetAssignment, DatabaseAssetAssignment>
{
    public DomainAssetAssignment ToDomain(DatabaseAssetAssignment databaseModel)
    {
        return new DomainAssetAssignment
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            AssetId = databaseModel.AssetId,
            RoomId = databaseModel.RoomId,
            BedId = databaseModel.BedId,
            Status = databaseModel.Status,
            Note = databaseModel.Note,
            AssignedAt = databaseModel.AssignedAt,
        };
    }

    public DatabaseAssetAssignment ToDatabase(DomainAssetAssignment domainEntity)
    {
        return new DatabaseAssetAssignment
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            AssetId = domainEntity.AssetId,
            RoomId = domainEntity.RoomId,
            BedId = domainEntity.BedId,
            Status = domainEntity.Status,
            Note = domainEntity.Note,
            AssignedAt = domainEntity.AssignedAt,
        };
    }
}

internal sealed class BedMapper : IDatabaseModelMapper<DomainBed, DatabaseBed>
{
    public DomainBed ToDomain(DatabaseBed databaseModel)
    {
        return new DomainBed
        {
            Id = databaseModel.Id,
            RoomId = databaseModel.RoomId,
            BedNumber = databaseModel.BedNumber,
            LockerId = databaseModel.LockerId,
            BasePrice = databaseModel.BasePrice,
            Status = databaseModel.Status,
            CardToken = databaseModel.CardToken,
        };
    }

    public DatabaseBed ToDatabase(DomainBed domainEntity)
    {
        return new DatabaseBed
        {
            Id = domainEntity.Id,
            RoomId = domainEntity.RoomId,
            BedNumber = domainEntity.BedNumber,
            LockerId = domainEntity.LockerId,
            BasePrice = domainEntity.BasePrice,
            Status = domainEntity.Status,
            CardToken = domainEntity.CardToken,
        };
    }
}

internal sealed class BookingMapper : IDatabaseModelMapper<DomainBooking, DatabaseBooking>
{
    public DomainBooking ToDomain(DatabaseBooking databaseModel)
    {
        return new DomainBooking
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            PropertyId = databaseModel.PropertyId,
            RoomId = databaseModel.RoomId,
            BedId = databaseModel.BedId,
            GuestName = databaseModel.GuestName,
            GuestPhone = databaseModel.GuestPhone,
            GuestEmail = databaseModel.GuestEmail,
            BookingType = databaseModel.BookingType,
            CheckInDate = databaseModel.CheckInDate,
            CheckOutDate = databaseModel.CheckOutDate,
            Amount = databaseModel.Amount,
            PaymentStatus = databaseModel.PaymentStatus,
            PaymentTransactionId = databaseModel.PaymentTransactionId,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseBooking ToDatabase(DomainBooking domainEntity)
    {
        return new DatabaseBooking
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            PropertyId = domainEntity.PropertyId,
            RoomId = domainEntity.RoomId,
            BedId = domainEntity.BedId,
            GuestName = domainEntity.GuestName,
            GuestPhone = domainEntity.GuestPhone,
            GuestEmail = domainEntity.GuestEmail,
            BookingType = domainEntity.BookingType,
            CheckInDate = domainEntity.CheckInDate,
            CheckOutDate = domainEntity.CheckOutDate,
            Amount = domainEntity.Amount,
            PaymentStatus = domainEntity.PaymentStatus,
            PaymentTransactionId = domainEntity.PaymentTransactionId,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class BrokerMapper : IDatabaseModelMapper<DomainBroker, DatabaseBroker>
{
    public DomainBroker ToDomain(DatabaseBroker databaseModel)
    {
        return new DomainBroker
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            FullName = databaseModel.FullName,
            Phone = databaseModel.Phone,
            WalletBalance = databaseModel.WalletBalance,
            TotalCommissionEarned = databaseModel.TotalCommissionEarned,
        };
    }

    public DatabaseBroker ToDatabase(DomainBroker domainEntity)
    {
        return new DatabaseBroker
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            FullName = domainEntity.FullName,
            Phone = domainEntity.Phone,
            WalletBalance = domainEntity.WalletBalance,
            TotalCommissionEarned = domainEntity.TotalCommissionEarned,
        };
    }
}

internal sealed class ContractMapper : IDatabaseModelMapper<DomainContract, DatabaseContract>
{
    public DomainContract ToDomain(DatabaseContract databaseModel)
    {
        return new DomainContract
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            PropertyId = databaseModel.PropertyId,
            RoomId = databaseModel.RoomId,
            BedId = databaseModel.BedId,
            ResidentId = databaseModel.ResidentId,
            BrokerId = databaseModel.BrokerId,
            BookingId = databaseModel.BookingId,
            StartDate = databaseModel.StartDate,
            EndDate = databaseModel.EndDate,
            DepositAmount = databaseModel.DepositAmount,
            BrokerCommission = databaseModel.BrokerCommission,
            CommissionStatus = databaseModel.CommissionStatus,
            ContractPdfUrl = databaseModel.ContractPdfUrl,
            Status = databaseModel.Status,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseContract ToDatabase(DomainContract domainEntity)
    {
        return new DatabaseContract
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            PropertyId = domainEntity.PropertyId,
            RoomId = domainEntity.RoomId,
            BedId = domainEntity.BedId,
            ResidentId = domainEntity.ResidentId,
            BrokerId = domainEntity.BrokerId,
            BookingId = domainEntity.BookingId,
            StartDate = domainEntity.StartDate,
            EndDate = domainEntity.EndDate,
            DepositAmount = domainEntity.DepositAmount,
            BrokerCommission = domainEntity.BrokerCommission,
            CommissionStatus = domainEntity.CommissionStatus,
            ContractPdfUrl = domainEntity.ContractPdfUrl,
            Status = domainEntity.Status,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class InvoiceMapper : IDatabaseModelMapper<DomainInvoice, DatabaseInvoice>
{
    public DomainInvoice ToDomain(DatabaseInvoice databaseModel)
    {
        return new DomainInvoice
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            ContractId = databaseModel.ContractId,
            BookingId = databaseModel.BookingId,
            InvoicePeriod = databaseModel.InvoicePeriod,
            RoomPrice = databaseModel.RoomPrice,
            ServicesPrice = databaseModel.ServicesPrice,
            TotalAmount = databaseModel.TotalAmount,
            QrCodeUrl = databaseModel.QrCodeUrl,
            Status = databaseModel.Status,
            PaidAt = databaseModel.PaidAt,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseInvoice ToDatabase(DomainInvoice domainEntity)
    {
        return new DatabaseInvoice
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            ContractId = domainEntity.ContractId,
            BookingId = domainEntity.BookingId,
            InvoicePeriod = domainEntity.InvoicePeriod,
            RoomPrice = domainEntity.RoomPrice,
            ServicesPrice = domainEntity.ServicesPrice,
            TotalAmount = domainEntity.TotalAmount,
            QrCodeUrl = domainEntity.QrCodeUrl,
            Status = domainEntity.Status,
            PaidAt = domainEntity.PaidAt,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class ListingMapper : IDatabaseModelMapper<DomainListing, DatabaseListing>
{
    public DomainListing ToDomain(DatabaseListing databaseModel)
    {
        return new DomainListing
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            PropertyId = databaseModel.PropertyId,
            RoomId = databaseModel.RoomId,
            BedId = databaseModel.BedId,
            Title = databaseModel.Title,
            Description = databaseModel.Description,
            Amenities = databaseModel.Amenities,
            IsPublished = databaseModel.IsPublished,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseListing ToDatabase(DomainListing domainEntity)
    {
        return new DatabaseListing
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            PropertyId = domainEntity.PropertyId,
            RoomId = domainEntity.RoomId,
            BedId = domainEntity.BedId,
            Title = domainEntity.Title,
            Description = domainEntity.Description,
            Amenities = domainEntity.Amenities,
            IsPublished = domainEntity.IsPublished,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class ListingImageMapper : IDatabaseModelMapper<DomainListingImage, DatabaseListingImage>
{
    public DomainListingImage ToDomain(DatabaseListingImage databaseModel)
    {
        return new DomainListingImage
        {
            Id = databaseModel.Id,
            ListingId = databaseModel.ListingId,
            ImageUrl = databaseModel.ImageUrl,
            DisplayOrder = databaseModel.DisplayOrder,
            UploadedAt = databaseModel.UploadedAt,
        };
    }

    public DatabaseListingImage ToDatabase(DomainListingImage domainEntity)
    {
        return new DatabaseListingImage
        {
            Id = domainEntity.Id,
            ListingId = domainEntity.ListingId,
            ImageUrl = domainEntity.ImageUrl,
            DisplayOrder = domainEntity.DisplayOrder,
            UploadedAt = domainEntity.UploadedAt,
        };
    }
}

internal sealed class MaintenanceTicketMapper : IDatabaseModelMapper<DomainMaintenanceTicket, DatabaseMaintenanceTicket>
{
    public DomainMaintenanceTicket ToDomain(DatabaseMaintenanceTicket databaseModel)
    {
        return new DomainMaintenanceTicket
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            RoomId = databaseModel.RoomId,
            ResidentId = databaseModel.ResidentId,
            AssetAssignmentId = databaseModel.AssetAssignmentId,
            Category = databaseModel.Category,
            UserDescription = databaseModel.UserDescription,
            IncidentImageUrl = databaseModel.IncidentImageUrl,
            Status = databaseModel.Status,
            TechnicianId = databaseModel.TechnicianId,
            ResolvedImageUrl = databaseModel.ResolvedImageUrl,
            CreatedAt = databaseModel.CreatedAt,
            UpdatedAt = databaseModel.UpdatedAt,
        };
    }

    public DatabaseMaintenanceTicket ToDatabase(DomainMaintenanceTicket domainEntity)
    {
        return new DatabaseMaintenanceTicket
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            RoomId = domainEntity.RoomId,
            ResidentId = domainEntity.ResidentId,
            AssetAssignmentId = domainEntity.AssetAssignmentId,
            Category = domainEntity.Category,
            UserDescription = domainEntity.UserDescription,
            IncidentImageUrl = domainEntity.IncidentImageUrl,
            Status = domainEntity.Status,
            TechnicianId = domainEntity.TechnicianId,
            ResolvedImageUrl = domainEntity.ResolvedImageUrl,
            CreatedAt = domainEntity.CreatedAt,
            UpdatedAt = domainEntity.UpdatedAt,
        };
    }
}

internal sealed class PermissionMapper : IDatabaseModelMapper<DomainPermission, DatabasePermission>
{
    public DomainPermission ToDomain(DatabasePermission databaseModel)
    {
        return new DomainPermission
        {
            Id = databaseModel.Id,
            PermissionName = databaseModel.PermissionName,
            PermissionKey = databaseModel.PermissionKey,
            Module = databaseModel.Module,
        };
    }

    public DatabasePermission ToDatabase(DomainPermission domainEntity)
    {
        return new DatabasePermission
        {
            Id = domainEntity.Id,
            PermissionName = domainEntity.PermissionName,
            PermissionKey = domainEntity.PermissionKey,
            Module = domainEntity.Module,
        };
    }
}

internal sealed class PropertyMapper : IDatabaseModelMapper<DomainProperty, DatabaseProperty>
{
    public DomainProperty ToDomain(DatabaseProperty databaseModel)
    {
        return new DomainProperty
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            PropertyName = databaseModel.PropertyName,
            Address = databaseModel.Address,
            PropertyType = databaseModel.PropertyType,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseProperty ToDatabase(DomainProperty domainEntity)
    {
        return new DatabaseProperty
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            PropertyName = domainEntity.PropertyName,
            Address = domainEntity.Address,
            PropertyType = domainEntity.PropertyType,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class ResidentMapper : IDatabaseModelMapper<DomainResident, DatabaseResident>
{
    public DomainResident ToDomain(DatabaseResident databaseModel)
    {
        return new DomainResident
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            FullName = databaseModel.FullName,
            Phone = databaseModel.Phone,
            Email = databaseModel.Email,
            IdentityCardNumber = databaseModel.IdentityCardNumber,
            IdFrontImageUrl = databaseModel.IdFrontImageUrl,
            IdBackImageUrl = databaseModel.IdBackImageUrl,
            ProfileImageUrl = databaseModel.ProfileImageUrl,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseResident ToDatabase(DomainResident domainEntity)
    {
        return new DatabaseResident
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            FullName = domainEntity.FullName,
            Phone = domainEntity.Phone,
            Email = domainEntity.Email,
            IdentityCardNumber = domainEntity.IdentityCardNumber,
            IdFrontImageUrl = domainEntity.IdFrontImageUrl,
            IdBackImageUrl = domainEntity.IdBackImageUrl,
            ProfileImageUrl = domainEntity.ProfileImageUrl,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class RoleMapper : IDatabaseModelMapper<DomainRole, DatabaseRole>
{
    public DomainRole ToDomain(DatabaseRole databaseModel)
    {
        return new DomainRole
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            RoleName = databaseModel.RoleName,
            RoleKey = databaseModel.RoleKey,
            Description = databaseModel.Description,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseRole ToDatabase(DomainRole domainEntity)
    {
        return new DatabaseRole
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            RoleName = domainEntity.RoleName,
            RoleKey = domainEntity.RoleKey,
            Description = domainEntity.Description,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class RoomMapper : IDatabaseModelMapper<DomainRoom, DatabaseRoom>
{
    public DomainRoom ToDomain(DatabaseRoom databaseModel)
    {
        return new DomainRoom
        {
            Id = databaseModel.Id,
            PropertyId = databaseModel.PropertyId,
            RoomNumber = databaseModel.RoomNumber,
            Floor = databaseModel.Floor,
            BasePrice = databaseModel.BasePrice,
            Status = databaseModel.Status,
            MaxOccupants = databaseModel.MaxOccupants,
            RowVersion = databaseModel.RowVersion,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseRoom ToDatabase(DomainRoom domainEntity)
    {
        return new DatabaseRoom
        {
            Id = domainEntity.Id,
            PropertyId = domainEntity.PropertyId,
            RoomNumber = domainEntity.RoomNumber,
            Floor = domainEntity.Floor,
            BasePrice = domainEntity.BasePrice,
            Status = domainEntity.Status,
            MaxOccupants = domainEntity.MaxOccupants,
            RowVersion = domainEntity.RowVersion,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class RoomAvailabilityMapper : IDatabaseModelMapper<DomainRoomAvailability, DatabaseRoomAvailability>
{
    public DomainRoomAvailability ToDomain(DatabaseRoomAvailability databaseModel)
    {
        return new DomainRoomAvailability
        {
            Id = databaseModel.Id,
            RoomId = databaseModel.RoomId,
            StayDate = databaseModel.StayDate,
            DynamicPrice = databaseModel.DynamicPrice,
            IsBooked = databaseModel.IsBooked,
        };
    }

    public DatabaseRoomAvailability ToDatabase(DomainRoomAvailability domainEntity)
    {
        return new DatabaseRoomAvailability
        {
            Id = domainEntity.Id,
            RoomId = domainEntity.RoomId,
            StayDate = domainEntity.StayDate,
            DynamicPrice = domainEntity.DynamicPrice,
            IsBooked = domainEntity.IsBooked,
        };
    }
}

internal sealed class RoomImageMapper : IDatabaseModelMapper<DomainRoomImage, DatabaseRoomImage>
{
    public DomainRoomImage ToDomain(DatabaseRoomImage databaseModel)
    {
        return new DomainRoomImage
        {
            Id = databaseModel.Id,
            RoomId = databaseModel.RoomId,
            ImageUrl = databaseModel.ImageUrl,
            IsCover = databaseModel.IsCover,
            UploadedAt = databaseModel.UploadedAt,
        };
    }

    public DatabaseRoomImage ToDatabase(DomainRoomImage domainEntity)
    {
        return new DatabaseRoomImage
        {
            Id = domainEntity.Id,
            RoomId = domainEntity.RoomId,
            ImageUrl = domainEntity.ImageUrl,
            IsCover = domainEntity.IsCover,
            UploadedAt = domainEntity.UploadedAt,
        };
    }
}

internal sealed class SubscriptionPackageMapper : IDatabaseModelMapper<DomainSubscriptionPackage, DatabaseSubscriptionPackage>
{
    public DomainSubscriptionPackage ToDomain(DatabaseSubscriptionPackage databaseModel)
    {
        return new DomainSubscriptionPackage
        {
            Id = databaseModel.Id,
            PackageName = databaseModel.PackageName,
            PackageKey = databaseModel.PackageKey,
            PriceMonthly = databaseModel.PriceMonthly,
            MaxProperties = databaseModel.MaxProperties,
            MaxRooms = databaseModel.MaxRooms,
            TokenGiftMonthly = databaseModel.TokenGiftMonthly,
            AllowCustomRoles = databaseModel.AllowCustomRoles,
            AllowAiFeatures = databaseModel.AllowAiFeatures,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseSubscriptionPackage ToDatabase(DomainSubscriptionPackage domainEntity)
    {
        return new DatabaseSubscriptionPackage
        {
            Id = domainEntity.Id,
            PackageName = domainEntity.PackageName,
            PackageKey = domainEntity.PackageKey,
            PriceMonthly = domainEntity.PriceMonthly,
            MaxProperties = domainEntity.MaxProperties,
            MaxRooms = domainEntity.MaxRooms,
            TokenGiftMonthly = domainEntity.TokenGiftMonthly,
            AllowCustomRoles = domainEntity.AllowCustomRoles,
            AllowAiFeatures = domainEntity.AllowAiFeatures,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}

internal sealed class TechnicianMapper : IDatabaseModelMapper<DomainTechnician, DatabaseTechnician>
{
    public DomainTechnician ToDomain(DatabaseTechnician databaseModel)
    {
        return new DomainTechnician
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            FullName = databaseModel.FullName,
            Phone = databaseModel.Phone,
            Specialty = databaseModel.Specialty,
            IsAvailable = databaseModel.IsAvailable,
        };
    }

    public DatabaseTechnician ToDatabase(DomainTechnician domainEntity)
    {
        return new DatabaseTechnician
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            FullName = domainEntity.FullName,
            Phone = domainEntity.Phone,
            Specialty = domainEntity.Specialty,
            IsAvailable = domainEntity.IsAvailable,
        };
    }
}

internal sealed class TenantMapper : IDatabaseModelMapper<DomainTenant, DatabaseTenant>
{
    public DomainTenant ToDomain(DatabaseTenant databaseModel)
    {
        return new DomainTenant
        {
            Id = databaseModel.Id,
            PackageId = databaseModel.PackageId,
            BusinessName = databaseModel.BusinessName,
            TaxCode = databaseModel.TaxCode,
            OwnerEmail = databaseModel.OwnerEmail,
            OwnerPhone = databaseModel.OwnerPhone,
            SubscriptionStatus = databaseModel.SubscriptionStatus,
            TokenBalance = databaseModel.TokenBalance,
            CreatedAt = databaseModel.CreatedAt,
            UpdatedAt = databaseModel.UpdatedAt,
        };
    }

    public DatabaseTenant ToDatabase(DomainTenant domainEntity)
    {
        return new DatabaseTenant
        {
            Id = domainEntity.Id,
            PackageId = domainEntity.PackageId,
            BusinessName = domainEntity.BusinessName,
            TaxCode = domainEntity.TaxCode,
            OwnerEmail = domainEntity.OwnerEmail,
            OwnerPhone = domainEntity.OwnerPhone,
            SubscriptionStatus = domainEntity.SubscriptionStatus,
            TokenBalance = domainEntity.TokenBalance,
            CreatedAt = domainEntity.CreatedAt,
            UpdatedAt = domainEntity.UpdatedAt,
        };
    }
}

internal sealed class UserMapper : IDatabaseModelMapper<DomainUser, DatabaseUser>
{
    public DomainUser ToDomain(DatabaseUser databaseModel)
    {
        return new DomainUser
        {
            Id = databaseModel.Id,
            TenantId = databaseModel.TenantId,
            FullName = databaseModel.FullName,
            Email = databaseModel.Email,
            Phone = databaseModel.Phone,
            PasswordHash = databaseModel.PasswordHash,
            IsActive = databaseModel.IsActive,
            CreatedAt = databaseModel.CreatedAt,
        };
    }

    public DatabaseUser ToDatabase(DomainUser domainEntity)
    {
        return new DatabaseUser
        {
            Id = domainEntity.Id,
            TenantId = domainEntity.TenantId,
            FullName = domainEntity.FullName,
            Email = domainEntity.Email,
            Phone = domainEntity.Phone,
            PasswordHash = domainEntity.PasswordHash,
            IsActive = domainEntity.IsActive,
            CreatedAt = domainEntity.CreatedAt,
        };
    }
}
