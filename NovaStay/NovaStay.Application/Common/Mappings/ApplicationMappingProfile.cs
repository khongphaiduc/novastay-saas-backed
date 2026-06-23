using AutoMapper;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;

namespace NovaStay.Application.Common.Mappings;

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

        CreateMap<AssetEntity, AssetDto>().ReverseMap();
        CreateMap<AssetAssignmentEntity, AssetAssignmentDto>().ReverseMap();
        CreateMap<BookingEntity, BookingDto>().ReverseMap();
        CreateMap<BrokerEntity, BrokerDto>().ReverseMap();
        CreateMap<ContractEntity, ContractDto>().ReverseMap();
        CreateMap<InvoiceEntity, InvoiceDto>().ReverseMap();
        CreateMap<MaintenanceTicketEntity, MaintenanceTicketDto>().ReverseMap();
        CreateMap<PermissionEntity, PermissionDto>().ReverseMap();
        CreateMap<PropertyEntity, PropertyDto>().ReverseMap();
        CreateMap<ResidentEntity, ResidentDto>().ReverseMap();
        CreateMap<ResidentMembershipEntity, ResidentMembershipDto>().ReverseMap();
        CreateMap<RoleEntity, RoleDto>().ReverseMap();
        CreateMap<RoomEntity, RoomDto>()
            .ForMember(destination => destination.Images, options => options.Ignore())
            .ReverseMap();
        CreateMap<RoomAvailabilityEntity, RoomAvailabilityDto>().ReverseMap();
        CreateMap<RoomImageEntity, RoomImageDto>().ReverseMap();
        CreateMap<SubscriptionPackageEntity, SubscriptionPackageDto>().ReverseMap();
        CreateMap<TechnicianEntity, TechnicianDto>().ReverseMap();
        CreateMap<AccountEntity, AccountDto>().ReverseMap();
        CreateMap<OrganizationEntity, OrganizationDto>().ReverseMap();
        CreateMap<AccountRefreshTokenEntity, AccountRefreshTokenDto>().ReverseMap();
        CreateMap<StaffUserEntity, StaffUserDto>().ReverseMap();
    }
}
