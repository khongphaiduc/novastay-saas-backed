using AutoMapper;
using NovaStay.Application.Common.Mappings;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using NovaStay.Infrastructure.Persistence.Mapping;
using Microsoft.Extensions.Logging.Abstractions;
using DatabaseOrganization = NovaStay.Infrastructure.Models.Organization;

namespace NovaStay.UnitTests.Infrastructure;

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
    public void OrganizationDatabaseModel_Maps_To_OrganizationEntity()
    {
        var databaseModel = new DatabaseOrganization
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

        var entity = _mapper.Map<OrganizationEntity>(databaseModel);

        Assert.Equal(databaseModel.Id, entity.Id);
        Assert.Equal(new EntityName(databaseModel.BusinessName), entity.BusinessName);
        Assert.Equal(new EmailAddress(databaseModel.OwnerEmail), entity.OwnerEmail);
        Assert.Equal(new PhoneNumber(databaseModel.OwnerPhone), entity.OwnerPhone);
        Assert.Equal(new Status(databaseModel.SubscriptionStatus), entity.SubscriptionStatus);
    }

    [Fact]
    public void OrganizationEntity_Maps_To_OrganizationDatabaseModel()
    {
        var entity = new OrganizationEntity
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

        var databaseModel = _mapper.Map<DatabaseOrganization>(entity);

        Assert.Equal(entity.Id, databaseModel.Id);
        Assert.Equal("Host Demo", databaseModel.BusinessName);
        Assert.Equal("owner@example.com", databaseModel.OwnerEmail);
        Assert.Equal("0900000000", databaseModel.OwnerPhone);
        Assert.Equal("Active", databaseModel.SubscriptionStatus);
    }

    [Fact]
    public void InvitationLinkEntity_Maps_To_InvitationLinkDatabaseModel()
    {
        var entity = new InvitationLinkEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Token = "abcdef123456",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            MaxUses = 10,
            UsedCount = 2,
            IsActive = true,
            Note = "Test invite link"
        };

        var dbModel = _mapper.Map<NovaStay.Infrastructure.Models.InvitationLink>(entity);

        Assert.Equal(entity.Id, dbModel.Id);
        Assert.Equal(entity.OrganizationId, dbModel.OrganizationId);
        Assert.Equal(entity.Token, dbModel.Token);
        Assert.Equal(entity.ExpiresAt, dbModel.ExpiresAt);
        Assert.Equal(entity.MaxUses, dbModel.MaxUses);
        Assert.Equal(entity.UsedCount, dbModel.UsedCount);
        Assert.Equal(entity.IsActive, dbModel.IsActive);
        Assert.Equal(entity.Note, dbModel.Note);
    }

    [Fact]
    public void InvitationLinkDatabaseModel_Maps_To_InvitationLinkEntity()
    {
        var dbModel = new NovaStay.Infrastructure.Models.InvitationLink
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Token = "abcdef123456",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            MaxUses = 10,
            UsedCount = 2,
            IsActive = true,
            Note = "Test invite link"
        };

        var entity = _mapper.Map<InvitationLinkEntity>(dbModel);

        Assert.Equal(dbModel.Id, entity.Id);
        Assert.Equal(dbModel.OrganizationId, entity.OrganizationId);
        Assert.Equal(dbModel.Token, entity.Token);
        Assert.Equal(dbModel.ExpiresAt, entity.ExpiresAt);
        Assert.Equal(dbModel.MaxUses, entity.MaxUses);
        Assert.Equal(dbModel.UsedCount, entity.UsedCount);
        Assert.Equal(dbModel.IsActive, entity.IsActive);
        Assert.Equal(dbModel.Note, entity.Note);
    }
}
