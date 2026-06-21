using AutoMapper;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using Microsoft.Extensions.Logging.Abstractions;

namespace NovaStay.UnitTests.Application;

public class ApplicationMappingProfileTests
{
    private readonly IMapper _mapper;

    public ApplicationMappingProfileTests()
    {
        var configuration = new MapperConfiguration(
            configuration => configuration.AddProfile<ApplicationMappingProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void OrganizationDto_Maps_To_Organization()
    {
        var dto = new OrganizationDto
        {
            Id = Guid.NewGuid(),
            PackageId = Guid.NewGuid(),
            BusinessName = "Host Demo",
            OwnerEmail = "owner@example.com",
            OwnerPhone = "0900000000",
            SubscriptionStatus = "Active",
            TokenBalance = 100
        };

        var Organization = _mapper.Map<OrganizationEntity>(dto);

        Assert.Equal(dto.Id, Organization.Id);
        Assert.Equal(new EntityName(dto.BusinessName), Organization.BusinessName);
        Assert.Equal(new EmailAddress(dto.OwnerEmail), Organization.OwnerEmail);
        Assert.Equal(new PhoneNumber(dto.OwnerPhone), Organization.OwnerPhone);
        Assert.Equal(new Status(dto.SubscriptionStatus), Organization.SubscriptionStatus);
    }

    [Fact]
    public void Room_Maps_To_RoomDto()
    {
        var room = new RoomEntity
        {
            Id = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            RoomNumber = new Code("A101"),
            Floor = 1,
            BasePrice = new Money(2500000m),
            Status = new Status("Available"),
            MaxOccupants = 2
        };

        var dto = _mapper.Map<RoomDto>(room);

        Assert.Equal(room.Id, dto.Id);
        Assert.Equal("A101", dto.RoomNumber);
        Assert.Equal(2500000m, dto.BasePrice);
        Assert.Equal("Available", dto.Status);
    }
}
