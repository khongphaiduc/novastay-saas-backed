using AutoMapper;
using Host.Application.DTOs;
using Host.Domain.Entities;
using Host.Domain.ValueObject;

namespace Host.Application.Common.Mappings;

public sealed class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<string, Code>().ConvertUsing(value => new Code(value));
        CreateMap<Code, string>().ConvertUsing(value => value.Value);
        CreateMap<string, EmailAddress>().ConvertUsing(value => new EmailAddress(value));
        CreateMap<EmailAddress, string>().ConvertUsing(value => value.Value);
        CreateMap<string, EntityName>().ConvertUsing(value => new EntityName(value));
        CreateMap<EntityName, string>().ConvertUsing(value => value.Value);
        CreateMap<decimal, Money>().ConvertUsing(value => new Money(value));
        CreateMap<Money, decimal>().ConvertUsing(value => value.Amount);
        CreateMap<string, PhoneNumber>().ConvertUsing(value => new PhoneNumber(value));
        CreateMap<PhoneNumber, string>().ConvertUsing(value => value.Value);
        CreateMap<string, Status>().ConvertUsing(value => new Status(value));
        CreateMap<Status, string>().ConvertUsing(value => value.Value);

        CreateMap<Asset, AssetDto>().ReverseMap();
        CreateMap<AssetAssignment, AssetAssignmentDto>().ReverseMap();
        CreateMap<Bed, BedDto>().ReverseMap();
        CreateMap<Booking, BookingDto>().ReverseMap();
        CreateMap<Broker, BrokerDto>().ReverseMap();
        CreateMap<Contract, ContractDto>().ReverseMap();
        CreateMap<Invoice, InvoiceDto>().ReverseMap();
        CreateMap<Listing, ListingDto>().ReverseMap();
        CreateMap<ListingImage, ListingImageDto>().ReverseMap();
        CreateMap<MaintenanceTicket, MaintenanceTicketDto>().ReverseMap();
        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<Property, PropertyDto>().ReverseMap();
        CreateMap<Resident, ResidentDto>().ReverseMap();
        CreateMap<Role, RoleDto>().ReverseMap();
        CreateMap<Room, RoomDto>().ReverseMap();
        CreateMap<RoomAvailability, RoomAvailabilityDto>().ReverseMap();
        CreateMap<RoomImage, RoomImageDto>().ReverseMap();
        CreateMap<SubscriptionPackage, SubscriptionPackageDto>().ReverseMap();
        CreateMap<Technician, TechnicianDto>().ReverseMap();
        CreateMap<Tenant, TenantDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
    }
}
