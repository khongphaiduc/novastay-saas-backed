using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class PropertyServiceTests : IAsyncLifetime
{
    private readonly HostContext _db = new(
        new DbContextOptionsBuilder<HostContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _organizationId = Guid.NewGuid();
    private ServiceProvider _provider = null!;
    private AsyncServiceScope _scope;
    private IPropertyService _propertyService = null!;

    public async Task InitializeAsync()
    {
        _db.Organizations.Add(new Organization
        {
            Id = _organizationId,
            OwnerAccountId = _ownerId,
            BusinessName = "Test Org",
            BusinessArea = "BoardingHouse",
            OwnerEmail = "owner@example.invalid",
            OwnerPhone = "0900000000",
            SubscriptionStatus = "Active"
        });
        await _db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SQLString"] = "not-used" })
            .Build();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(config);

        services.AddAutoMapper(c =>
        {
            c.AddProfile<ApplicationMappingProfile>();
            c.AddProfile<DatabaseModelMappingProfile>();
        });

        services.AddInfrastructure(config);
        services.AddScoped(_ => _db);

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateAsyncScope();
        _propertyService = _scope.ServiceProvider.GetRequiredService<IPropertyService>();
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task DeletePropertyAsync_SoftDeletesProperty_WhenAllRoomsAreSoftDeleted()
    {
        var propertyId = Guid.NewGuid();
        _db.Properties.Add(new Property
        {
            Id = propertyId,
            OrganizationId = _organizationId,
            PropertyName = "Cơ sở A",
            Address = "123 Test",
            PropertyType = "BoardingHouse",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });
        _db.Rooms.Add(new Room
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            RoomNumber = "101",
            Floor = 1,
            BasePrice = 2000000,
            Status = "Available",
            IsDeleted = true,
            RowVersion = []
        });
        await _db.SaveChangesAsync();

        await _propertyService.DeletePropertyAsync(_organizationId, propertyId);

        var dbProperty = await _db.Properties.AsNoTracking().SingleAsync(p => p.Id == propertyId);
        Assert.Equal("Deleted", dbProperty.Status);

        var activeList = await _propertyService.GetPropertiesAsync(_organizationId);
        Assert.Empty(activeList.Items);
    }

    [Fact]
    public async Task DeletePropertyAsync_ThrowsInvalidOperationException_WhenActiveRoomExists()
    {
        var propertyId = Guid.NewGuid();
        _db.Properties.Add(new Property
        {
            Id = propertyId,
            OrganizationId = _organizationId,
            PropertyName = "Cơ sở B",
            Address = "456 Test",
            PropertyType = "BoardingHouse",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });
        _db.Rooms.Add(new Room
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            RoomNumber = "102",
            Floor = 1,
            BasePrice = 2000000,
            Status = "Available",
            IsDeleted = false,
            RowVersion = []
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _propertyService.DeletePropertyAsync(_organizationId, propertyId));

        var dbProperty = await _db.Properties.AsNoTracking().SingleAsync(p => p.Id == propertyId);
        Assert.Equal("Active", dbProperty.Status);
    }

    [Fact]
    public async Task DeletePropertyAsync_ThrowsKeyNotFoundException_WhenAlreadySoftDeleted()
    {
        var propertyId = Guid.NewGuid();
        _db.Properties.Add(new Property
        {
            Id = propertyId,
            OrganizationId = _organizationId,
            PropertyName = "Cơ sở C",
            Address = "789 Test",
            PropertyType = "BoardingHouse",
            Status = "Deleted",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _propertyService.DeletePropertyAsync(_organizationId, propertyId));
    }

    [Fact]
    public async Task UpdatePropertyAsync_PreservesActiveStatus_WhenRequestStatusIsEmpty()
    {
        var propertyId = Guid.NewGuid();
        _db.Properties.Add(new Property
        {
            Id = propertyId,
            OrganizationId = _organizationId,
            PropertyName = "Cơ sở D",
            Address = "101 Test",
            PropertyType = "BoardingHouse",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var updated = await _propertyService.UpdatePropertyAsync(
            _organizationId,
            propertyId,
            new UpdatePropertyRequest
            {
                PropertyName = "Cơ sở D mới",
                Address = "102 Test",
                PropertyType = "Apartment",
                Status = string.Empty
            });

        Assert.Equal("Active", updated.Status);
        Assert.Equal("Cơ sở D mới", updated.PropertyName);
    }
}
