using AutoMapper;
using Host.Application.Common.Mappings;
using Host.Application.DTOs;
using Host.Domain.Entities;
using Host.Domain.ValueObject;
using Microsoft.Extensions.Logging.Abstractions;

namespace Host.UnitTests.Application;

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
    public void TenantDto_Maps_To_Tenant()
    {
        var dto = new TenantDto
        {
            Id = Guid.NewGuid(),
            PackageId = Guid.NewGuid(),
            BusinessName = "Host Demo",
            OwnerEmail = "owner@example.com",
            OwnerPhone = "0900000000",
            SubscriptionStatus = "Active",
            TokenBalance = 100
        };

        var tenant = _mapper.Map<Tenant>(dto);

        Assert.Equal(dto.Id, tenant.Id);
        Assert.Equal(new EntityName(dto.BusinessName), tenant.BusinessName);
        Assert.Equal(new EmailAddress(dto.OwnerEmail), tenant.OwnerEmail);
        Assert.Equal(new PhoneNumber(dto.OwnerPhone), tenant.OwnerPhone);
        Assert.Equal(new Status(dto.SubscriptionStatus), tenant.SubscriptionStatus);
    }

    [Fact]
    public void Room_Maps_To_RoomDto()
    {
        var room = new Room
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
