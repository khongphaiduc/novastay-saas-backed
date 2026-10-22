using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.ServicesImple;
using PropertyServiceModel = NovaStay.Infrastructure.Models.PropertyService;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class UtilityServiceTests : IDisposable
{
    private readonly HostContext _context;
    private readonly UtilityService _service;
    private readonly Room _room;
    private readonly PropertyServiceModel _propertyService;

    public UtilityServiceTests()
    {
        _context = new HostContext(new DbContextOptionsBuilder<HostContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _service = new UtilityService(_context);
        var propertyId = Guid.NewGuid();
        _room = new Room { Id = Guid.NewGuid(), PropertyId = propertyId, RoomNumber = "101", Status = "Occupied", RowVersion = [] };
        _propertyService = new PropertyServiceModel { Id = Guid.NewGuid(), PropertyId = propertyId, ServiceName = "Điện", DefaultPrice = 3000, Unit = "kWh", IsActive = true };
        _context.AddRange(_room, _propertyService);
        _context.SaveChanges();
    }

    private CreateUtilityMeterRequest MeterRequest() => new()
    {
        RoomId = _room.Id, PropertyServiceId = _propertyService.Id,
        MeterType = "Electric", Unit = "kWh", InitialReading = 100
    };

    [Fact]
    public async Task CreateMeter_ThenRecord_UsesInitialReadingAndReturnsPersistedHistory()
    {
        var meter = await _service.CreateMeterAsync(MeterRequest());
        var first = await _service.RecordReadingAsync(meter.Id, new() { BillingPeriod = "2026-10", CurrentReading = 125.5m });
        Assert.Equal(100, first.PreviousReading);
        Assert.Equal(25.5m, first.Consumption);
        Assert.Equal(76500m, first.Amount);
        var second = await _service.RecordReadingAsync(meter.Id, new() { BillingPeriod = "2026-11", CurrentReading = 140 });
        Assert.Equal(125.5m, second.PreviousReading);
        var loaded = Assert.Single(await _service.GetMetersByRoomAsync(_room.Id));
        Assert.Equal("Điện", loaded.ServiceName);
        Assert.Equal(2, loaded.Readings.Count);
        Assert.Equal("2026-11", loaded.LatestReading!.BillingPeriod);
        Assert.Equal(2, await _context.UtilityReadings.CountAsync());
    }

    [Fact]
    public async Task CreateMeter_RejectsDuplicateRoomService()
    {
        await _service.CreateMeterAsync(MeterRequest());
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateMeterAsync(MeterRequest()));
        Assert.Equal(1, await _context.UtilityMeters.CountAsync());
    }

    [Fact]
    public async Task RecordReading_UsesConfiguredElectricPriceInsteadOfLegacyZeroTariff()
    {
        _propertyService.DefaultPrice = 3200;
        _context.UtilityTariffs.Add(new UtilityTariff
        {
            Id = Guid.NewGuid(), PropertyServiceId = _propertyService.Id,
            EffectiveFrom = new DateTime(2020, 1, 1), UnitPrice = 0, PricingMode = "Flat"
        });
        await _context.SaveChangesAsync();
        var meter = await _service.CreateMeterAsync(MeterRequest());
        var reading = await _service.RecordReadingAsync(meter.Id, new() { BillingPeriod = "2026-10", CurrentReading = 120 });
        Assert.Equal(20, reading.Consumption);
        Assert.Equal(3200, reading.UnitPrice);
        Assert.Equal(64000, reading.Amount);
    }

    [Fact]
    public async Task RecordReading_RespectsRoomSpecificPrice()
    {
        _context.RoomServices.Add(new NovaStay.Infrastructure.Models.RoomService
        {
            Id = Guid.NewGuid(), RoomId = _room.Id, PropertyServiceId = _propertyService.Id,
            PriceOverride = 3200, IsActive = true
        });
        await _context.SaveChangesAsync();
        var meter = await _service.CreateMeterAsync(MeterRequest());
        var reading = await _service.RecordReadingAsync(meter.Id, new() { BillingPeriod = "2026-10", CurrentReading = 120 });
        Assert.Equal(3200, reading.UnitPrice);
        Assert.Equal(64000, reading.Amount);
    }

    [Fact]
    public async Task CreateMeter_RejectsServiceFromAnotherProperty()
    {
        _propertyService.PropertyId = Guid.NewGuid();
        await _context.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateMeterAsync(MeterRequest()));
    }

    [Fact]
    public async Task CreateMeter_RejectsInactiveServiceAndNegativeInitialValue()
    {
        var request = MeterRequest();
        request.InitialReading = -1;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateMeterAsync(request));
        _propertyService.IsActive = false;
        await _context.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateMeterAsync(MeterRequest()));
    }

    [Theory]
    [InlineData("2026-13")]
    [InlineData("2026-1")]
    [InlineData("")]
    public async Task RecordReading_RejectsInvalidPeriod(string period)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RecordReadingAsync(Guid.NewGuid(),
            new() { BillingPeriod = period, CurrentReading = 101 }));
    }

    [Fact]
    public async Task RecordReading_RejectsDuplicateDecreasingAndBackdatedReadings()
    {
        var meter = await _service.CreateMeterAsync(MeterRequest());
        await _service.RecordReadingAsync(meter.Id, new() { BillingPeriod = "2026-10", CurrentReading = 150 });
        foreach (var request in new[]
        {
            new RecordUtilityReadingRequest { BillingPeriod = "2026-10", CurrentReading = 160 },
            new RecordUtilityReadingRequest { BillingPeriod = "2026-11", CurrentReading = 140 },
            new RecordUtilityReadingRequest { BillingPeriod = "2026-09", CurrentReading = 160 }
        })
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RecordReadingAsync(meter.Id, request));
        Assert.Equal(1, await _context.UtilityReadings.CountAsync());
    }

    [Fact]
    public async Task RecordReading_RejectsInactiveMeter()
    {
        var meter = await _service.CreateMeterAsync(MeterRequest());
        (await _context.UtilityMeters.SingleAsync()).IsActive = false;
        await _context.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RecordReadingAsync(meter.Id,
            new() { BillingPeriod = "2026-10", CurrentReading = 120 }));
    }

    public void Dispose() => _context.Dispose();
}
