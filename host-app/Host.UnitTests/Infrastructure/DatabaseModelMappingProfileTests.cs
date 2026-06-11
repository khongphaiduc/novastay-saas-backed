using AutoMapper;
using Host.Application.Common.Mappings;
using Host.Domain.Entities;
using Host.Domain.ValueObject;
using Host.Infrastructure.Persistence.Mapping;
using Microsoft.Extensions.Logging.Abstractions;
using DatabaseTenant = Host.Infrastructure.Models.Tenant;

namespace Host.UnitTests.Infrastructure;

public sealed class DatabaseModelMappingProfileTests
{
    private readonly IMapper _mapper;

    public DatabaseModelMappingProfileTests()
    {
        var configuration = new MapperConfiguration(
            configuration =>
            {
                configuration.AddProfile<ApplicationMappingProfile>();
                configuration.AddProfile<DatabaseModelMappingProfile>();
            },
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void TenantDatabaseModel_Maps_To_TenantEntity()
    {
        var databaseModel = new DatabaseTenant
        {
            Id = Guid.NewGuid(),
            PackageId = Guid.NewGuid(),
            BusinessName = "Host Demo",
            OwnerEmail = "owner@example.com",
            OwnerPhone = "0900000000",
            SubscriptionStatus = "Active",
            TokenBalance = 100,
            CreatedAt = DateTime.UtcNow
        };

        var entity = _mapper.Map<TenantEntity>(databaseModel);

        Assert.Equal(databaseModel.Id, entity.Id);
        Assert.Equal(new EntityName(databaseModel.BusinessName), entity.BusinessName);
        Assert.Equal(new EmailAddress(databaseModel.OwnerEmail), entity.OwnerEmail);
        Assert.Equal(new PhoneNumber(databaseModel.OwnerPhone), entity.OwnerPhone);
        Assert.Equal(new Status(databaseModel.SubscriptionStatus), entity.SubscriptionStatus);
    }

    [Fact]
    public void TenantEntity_Maps_To_TenantDatabaseModel()
    {
        var entity = new TenantEntity
        {
            Id = Guid.NewGuid(),
            PackageId = Guid.NewGuid(),
            BusinessName = new EntityName("Host Demo"),
            OwnerEmail = new EmailAddress("owner@example.com"),
            OwnerPhone = new PhoneNumber("0900000000"),
            SubscriptionStatus = new Status("Active"),
            TokenBalance = 100,
            CreatedAt = DateTime.UtcNow
        };

        var databaseModel = _mapper.Map<DatabaseTenant>(entity);

        Assert.Equal(entity.Id, databaseModel.Id);
        Assert.Equal("Host Demo", databaseModel.BusinessName);
        Assert.Equal("owner@example.com", databaseModel.OwnerEmail);
        Assert.Equal("0900000000", databaseModel.OwnerPhone);
        Assert.Equal("Active", databaseModel.SubscriptionStatus);
    }
}
