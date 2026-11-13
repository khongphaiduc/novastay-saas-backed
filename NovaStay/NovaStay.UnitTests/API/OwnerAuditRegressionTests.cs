using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStay.API.Controllers;
using NovaStay.API.Security;
using NovaStay.Application.Common;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;

namespace NovaStay.UnitTests.API;

public sealed class OwnerAuditRegressionTests : IAsyncLifetime
{
    private readonly HostContext db = new(new DbContextOptionsBuilder<HostContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly Guid owner = Guid.NewGuid(), other = Guid.NewGuid(), org = Guid.NewGuid(), property = Guid.NewGuid();
    private readonly List<Contract> leases = [];
    public Task InitializeAsync()
    {
        db.Organizations.Add(new Organization { Id = org, OwnerAccountId = owner, BusinessName = "Test", BusinessArea = "Test", OwnerEmail = "owner@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Trial" });
        db.Properties.Add(new Property { Id = property, OrganizationId = org, PropertyName = "Test", Address = "Test", PropertyType = "BoardingHouse", Status = "Active" });
        for (var i = 0; i < 25; i++)
        {
            var account = new Account { Id = Guid.NewGuid(), AccountType = "Resident", CustomerName = "Test", Phone = "000", IsActive = true };
            var person = new Resident { Id = Guid.NewGuid(), AccountId = account.Id, FullName = "Test", Phone = "000", Sex = "Other" };
            var room = new Room { Id = Guid.NewGuid(), PropertyId = property, RoomNumber = $"P{i:00}", MaxOccupants = 1, Status = "Occupied", BasePrice = 3000000, RowVersion = [] };
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var lease = new Contract { Id = Guid.NewGuid(), OrganizationId = org, PropertyId = property, RoomId = room.Id, ResidentId = person.Id,
                StartDate = today.AddYears(-1), EndDate = today.AddDays(i == 0 ? -1 : i == 1 ? 15 : 90), Status = "Active", CreatedAt = DateTime.UtcNow };
            leases.Add(lease);
            db.AddRange(account, person, room, lease, new ResidentMembership { Id = Guid.NewGuid(), AccountId = account.Id, ResidentId = person.Id,
                OrganizationId = org, MembershipCode = Guid.NewGuid().ToString("N"), Status = "ACTIVE" });
        }
        return db.SaveChangesAsync();
    }
    public Task DisposeAsync() => db.DisposeAsync().AsTask();
    private ControllerContext Context(Guid user) => new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
        new Claim(ClaimTypes.NameIdentifier, user.ToString()), new Claim(ClaimTypes.Role, "BusinessOwner") }, "test")) } };

    [Theory]
    [InlineData("Rooms", "id")]
    [InlineData("Rooms", "propertyId")]
    [InlineData("Contracts", "id")]
    [InlineData("Contracts", "organizationId")]
    [InlineData("Residents", "id")]
    public async Task OwnerGuard_RejectsOtherOwnerBeforeAction(string controller, string key)
    {
        var id = key == "propertyId" ? property : key == "organizationId" ? org : controller == "Rooms" ? leases[0].RoomId : controller == "Residents" ? leases[0].ResidentId : leases[0].Id;
        var route = new RouteData(); route.Values["controller"] = controller;
        var action = new ActionContext(Context(other).HttpContext, route, new ActionDescriptor());
        var executing = new ActionExecutingContext(action, [], new Dictionary<string, object?> { [key] = id }, new object());
        var called = false;
        await new OwnerDataFilter(db).OnActionExecutionAsync(executing, () => { called = true; return Task.FromResult(new ActionExecutedContext(action, [], new object())); });
        Assert.False(called); Assert.IsType<NotFoundObjectResult>(executing.Result);
    }
    [Fact]
    public async Task OwnerGuard_ValidatesBodyIdsAndAcceptsOwnData()
    {
        foreach (var user in new[] { other, owner })
        {
            var route = new RouteData(); route.Values["controller"] = "Contracts";
            var action = new ActionContext(Context(user).HttpContext, route, new ActionDescriptor());
            var executing = new ActionExecutingContext(action, [], new Dictionary<string, object?> { ["request"] = new CreateContractRequest {
                OrganizationId = org, PropertyId = property, RoomId = leases[0].RoomId, ResidentId = leases[0].ResidentId } }, new object());
            var called = false;
            await new OwnerDataFilter(db).OnActionExecutionAsync(executing, () => { called = true; return Task.FromResult(new ActionExecutedContext(action, [], new object())); });
            Assert.Equal(user == owner, called);
        }
    }
    [Fact]
    public async Task OwnerCanInviteUnassignedResidentWithoutExistingMembership()
    {
        var route = new RouteData(); route.Values["controller"] = "Organizations";
        var action = new ActionContext(Context(owner).HttpContext, route, new ActionDescriptor());
        var executing = new ActionExecutingContext(action, [], new Dictionary<string, object?> {
            ["organizationId"] = org, ["request"] = new InviteResidentToOrganizationRequest { ResidentId = Guid.NewGuid() } }, new object());
        var called = false;
        await new OwnerDataFilter(db).OnActionExecutionAsync(executing, () => { called = true; return Task.FromResult(new ActionExecutedContext(action, [], new object())); });
        Assert.True(called); // The invitation service still validates that the resident exists and is eligible.
    }
    [Fact]
    public async Task BodyCannotOverrideUnownedRouteOrganization()
    {
        var route = new RouteData(); route.Values["controller"] = "Contracts";
        var action = new ActionContext(Context(owner).HttpContext, route, new ActionDescriptor());
        var executing = new ActionExecutingContext(action, [], new Dictionary<string, object?> {
            ["organizationId"] = Guid.NewGuid(), ["request"] = new CreateContractRequest { OrganizationId = org, PropertyId = property,
                RoomId = leases[0].RoomId, ResidentId = leases[0].ResidentId } }, new object());
        var called = false;
        await new OwnerDataFilter(db).OnActionExecutionAsync(executing, () => { called = true; return Task.FromResult(new ActionExecutedContext(action, [], new object())); });
        Assert.False(called); Assert.IsType<NotFoundObjectResult>(executing.Result);
    }
    [Fact]
    public async Task Summary_IsAcrossAllPages_AndCannotBeReadByAnotherOwner()
    {
        var api = new OwnerStatisticsController(db) { ControllerContext = Context(owner) };
        var rooms = Assert.IsType<Dictionary<string, int>>(Assert.IsType<OkObjectResult>(await api.Rooms(property, default)).Value);
        Assert.Equal(25, rooms["All"]); Assert.Equal(25, rooms["Occupied"]);
        var contracts = Assert.IsType<OkObjectResult>(await api.Contracts(org, default));
        Assert.Equal(25, contracts.Value!.GetType().GetProperty("Total")!.GetValue(contracts.Value));
        api.ControllerContext = Context(other);
        Assert.IsType<NotFoundResult>(await api.Rooms(property, default));
        Assert.IsType<NotFoundResult>(await api.Contracts(org, default));
    }
    [Theory]
    [InlineData("Còn hiệu lực", 23)]
    [InlineData("Sắp hết hạn", 1)]
    [InlineData("Quá hạn", 1)]
    [InlineData("Đã thanh lý", 0)]
    public async Task ContractFilter_IsAppliedBeforeCountAndPaging(string filter, int expected)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["SQLString"] = "not-used" }).Build();
        var services = new ServiceCollection().AddLogging().AddSingleton<IConfiguration>(config);
        services.AddAutoMapper(c => { c.AddProfile<ApplicationMappingProfile>(); c.AddProfile<DatabaseModelMappingProfile>(); });
        services.AddInfrastructure(config); services.AddScoped(_ => db);
        await using var provider = services.BuildServiceProvider(); await using var scope = provider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IContractRepository>();
        var result = await repository.GetPagedContractsAsync(org, null, filter, null, 1, 10);
        Assert.Equal(expected, result.TotalCount); Assert.Equal(Math.Min(expected, 10), result.Items.Count);
    }
    [Fact]
    public async Task DeletingOccupiedRoomAndInvalidDatesAreRejected()
    {
        var room = await db.Rooms.SingleAsync(x => x.Id == leases[0].RoomId); room.IsDeleted = true;
        await Assert.ThrowsAsync<RoomOccupancyException>(() => db.SaveChangesAsync());
        room.IsDeleted = false; leases[0].EndDate = leases[0].StartDate;
        await Assert.ThrowsAsync<RoomOccupancyException>(() => db.SaveChangesAsync());
    }
    [Fact]
    public async Task PrimaryResidentCannotBeAssignedToAnotherRoom()
    {
        leases[1].ResidentId = leases[0].ResidentId;
        await Assert.ThrowsAsync<RoomOccupancyException>(() => db.SaveChangesAsync());
    }
}
