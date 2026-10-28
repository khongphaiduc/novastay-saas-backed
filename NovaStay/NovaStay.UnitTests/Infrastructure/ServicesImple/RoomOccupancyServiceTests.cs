using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStay.Application.Common;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class RoomOccupancyServiceTests : IDisposable
{
    private readonly HostContext db = new(new DbContextOptionsBuilder<HostContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly Guid owner = Guid.NewGuid(), org = Guid.NewGuid(), property = Guid.NewGuid();
    private readonly RoomOccupancyService service;

    public RoomOccupancyServiceTests()
    {
        service = new(db);
        db.Organizations.Add(new Organization { Id = org, OwnerAccountId = owner, BusinessName = "Test", BusinessArea = "Test", OwnerEmail = "owner@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Active" });
        db.Properties.Add(new Property { Id = property, OrganizationId = org, PropertyName = "Test rooms", Address = "Test", PropertyType = "BoardingHouse", Status = "Active" });
        db.SaveChanges();
    }

    private Resident Person(Guid? organization = null, bool active = true)
    {
        var account = new Account { Id = Guid.NewGuid(), AccountType = "Resident", CustomerName = "Test resident", Phone = "000", IsActive = active, CreatedAt = DateTime.UtcNow };
        var person = new Resident { Id = Guid.NewGuid(), AccountId = account.Id, FullName = "Test " + Guid.NewGuid().ToString("N")[..6], Sex = "Other", Phone = "000" };
        db.AddRange(account, person, new ResidentMembership { Id = Guid.NewGuid(), AccountId = account.Id, ResidentId = person.Id, OrganizationId = organization ?? org,
            MembershipCode = Guid.NewGuid().ToString("N")[..16], Status = "ACTIVE", CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
        return person;
    }

    private Contract Lease(Resident person, int capacity = 2, decimal rent = 3000000)
    {
        var room = new Room { Id = Guid.NewGuid(), PropertyId = property, RoomNumber = Guid.NewGuid().ToString("N")[..4],
            MaxOccupants = capacity, BasePrice = rent, Status = "Occupied", RowVersion = [] };
        var contract = new Contract { Id = Guid.NewGuid(), OrganizationId = org, PropertyId = property, RoomId = room.Id, ResidentId = person.Id,
            Status = "Active", StartDate = new(2026, 1, 1), EndDate = new(2027, 12, 31), CreatedAt = DateTime.UtcNow };
        db.AddRange(room, contract); db.SaveChanges();
        return contract;
    }

    [Fact]
    public async Task CoResident_SeesRoomButDoesNotBecomeInvoicePayer_AndCanLeaveAndRejoin()
    {
        var representative = Person(); var roommate = Person(); var lease = Lease(representative);
        var assigned = await service.Set(owner, lease.Id, [roommate.Id], default);
        Assert.Equal(2, assigned.Occupants.Count);
        Assert.Equal(representative.Id, Assert.Single(assigned.Occupants.Where(x => x.IsRepresentative)).ResidentId);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SQLString"] = "not-used", ["JWT:SecretKey"] = new string('t', 64), ["JWT:Issuer"] = "test", ["JWT:Audience"] = "test" }).Build();
        var services = new ServiceCollection().AddLogging().AddSingleton<IConfiguration>(configuration);
        services.AddAutoMapper(c => { c.AddProfile<ApplicationMappingProfile>(); c.AddProfile<DatabaseModelMappingProfile>(); });
        services.AddInfrastructure(configuration); services.AddScoped(_ => db);
        await using var provider = services.BuildServiceProvider(); await using var scope = provider.CreateAsyncScope();
        var residentService = scope.ServiceProvider.GetRequiredService<IResidentService>();
        Assert.Equal(lease.RoomId, (await residentService.GetMyRoomAsync(roommate.AccountId))!.RoomId);
        Assert.Empty(await residentService.GetMyContractsAsync(roommate.AccountId)); // No representative's private PDF is exposed.
        var billing = new BillingService(db);
        await billing.SaveBank(owner, "Test bank", "00000001", "Test owner", null, null, false);
        var bill = Assert.Single(await billing.Issue(owner, new IssueBillsRequest { PropertyId = property, Period = "2026-10", DueDate = new(2026, 10, 31), RoomIds = [lease.RoomId], ChargeKeys = ["rent"] }));
        Assert.Equal(3000000, bill.TotalAmount);
        Assert.Empty(await billing.ResidentBills(roommate.AccountId));
        Assert.Single(await billing.ResidentBills(representative.AccountId));

        await service.Set(owner, lease.Id, [], default);
        Assert.Null(await residentService.GetMyRoomAsync(roommate.AccountId));
        Assert.NotNull((await db.ContractCoResidents.SingleAsync()).LeftAt);
        await service.Set(owner, lease.Id, [roommate.Id], default);
        Assert.Single(db.ContractCoResidents);
        Assert.Null((await db.ContractCoResidents.SingleAsync()).LeftAt);
    }

    [Fact]
    public async Task WrongOwnerCannotReadOrChangeOccupantsOrCandidates()
    {
        var lease = Lease(Person()); var roommate = Person();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Get(Guid.NewGuid(), lease.Id, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Set(Guid.NewGuid(), lease.Id, [roommate.Id], default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Candidates(Guid.NewGuid(), lease.Id, null, 1, 20, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.List(Guid.NewGuid(), org, default));
        Assert.Empty(db.ContractCoResidents);
    }

    [Theory]
    [InlineData("other-organization")]
    [InlineData("inactive")]
    [InlineData("already-representative")]
    [InlineData("already-roommate")]
    public async Task InvalidAssignmentsAreRejectedWithoutPersistingNewOccupants(string kind)
    {
        var lease = Lease(Person());
        var person = Person(kind == "other-organization" ? Guid.NewGuid() : null, kind != "inactive");
        if (kind == "already-representative") Lease(person);
        if (kind == "already-roommate")
        {
            var other = Lease(Person()); await service.Set(owner, other.Id, [person.Id], default);
        }
        await Assert.ThrowsAsync<RoomOccupancyException>(() => service.Set(owner, lease.Id, [person.Id], default));
        db.ChangeTracker.Clear();
        Assert.False(await db.ContractCoResidents.AnyAsync(x => x.ContractId == lease.Id));
    }

    [Fact]
    public async Task CapacityDuplicateAndRepresentativeChecksAreEnforced()
    {
        var representative = Person(); var lease = Lease(representative); var first = Person(); var second = Person();
        await Assert.ThrowsAsync<RoomOccupancyException>(() => service.Set(owner, lease.Id, [first.Id, second.Id], default));
        await Assert.ThrowsAsync<ArgumentException>(() => service.Set(owner, lease.Id, [first.Id, first.Id], default));
        await Assert.ThrowsAsync<RoomOccupancyException>(() => service.Set(owner, lease.Id, [representative.Id], default));
        await service.Set(owner, lease.Id, [first.Id], default);
        var room = await db.Rooms.SingleAsync(r => r.Id == lease.RoomId); room.MaxOccupants = 1;
        await Assert.ThrowsAsync<RoomOccupancyException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Equal(2, (await db.Rooms.SingleAsync(r => r.Id == lease.RoomId)).MaxOccupants);
    }

    [Fact]
    public async Task ExistingCoResidentCannotAcquireAnotherPrimaryContract()
    {
        var person = Person(); var lease = Lease(Person());
        await service.Set(owner, lease.Id, [person.Id], default);
        await Assert.ThrowsAsync<RoomOccupancyException>(async () => { Lease(person); await Task.CompletedTask; });
        db.ChangeTracker.Clear();
        Assert.Single(db.Contracts);
    }

    [Fact]
    public async Task TerminatedContractReleasesPeople_AndRenewalCannotTakeAnAlreadyOccupiedRoom()
    {
        var person = Person(); var lease = Lease(Person());
        await service.Set(owner, lease.Id, [person.Id], default);
        var tracked = await db.Contracts.SingleAsync(c => c.Id == lease.Id); tracked.Status = "Terminated";
        await db.SaveChangesAsync();
        Lease(person);
        tracked.Status = "Active";
        await Assert.ThrowsAsync<RoomOccupancyException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Equal("Terminated", (await db.Contracts.SingleAsync(c => c.Id == lease.Id)).Status);
    }

    [Fact]
    public async Task CandidatesArePagedAndExcludeEveryoneAlreadyHoused()
    {
        var lease = Lease(Person()); var roommate = Person(); var eligible = Person();
        await service.Set(owner, lease.Id, [roommate.Id], default);
        var candidates = await service.Candidates(owner, lease.Id, eligible.FullName, 1, 1, default);
        Assert.Equal(1, candidates.TotalCount); Assert.Equal(eligible.Id, Assert.Single(candidates.Items).ResidentId);
        Assert.Equal(1, candidates.TotalPages);
        await Assert.ThrowsAsync<ArgumentException>(() => service.Candidates(owner, lease.Id, null, 0, 20, default));
    }

    [Fact]
    public async Task ThirtyOneResidentsInNineteenRooms_ProduceNineteenRentChargesTotallingFiftyMillion()
    {
        var people = Enumerable.Range(0, 31).Select(_ => Person()).ToArray();
        var cursor = 0;
        for (var i = 0; i < 19; i++)
        {
            var lease = Lease(people[cursor++], i < 12 ? 2 : 1, i < 12 ? 3000000 : 2000000);
            if (i < 12) await service.Set(owner, lease.Id, [people[cursor++].Id], default);
        }
        var rooms = await service.List(owner, org, default);
        Assert.Equal(19, rooms.Count);
        Assert.Equal(12, rooms.Count(r => r.Occupants.Count == 2));
        Assert.Equal(7, rooms.Count(r => r.Occupants.Count == 1));
        Assert.Equal(31, rooms.SelectMany(r => r.Occupants).Select(p => p.ResidentId).Distinct().Count());
        var preview = await new BillingService(db).Preview(owner, property, "2026-10");
        Assert.All(preview, room => Assert.Empty(room.Errors));
        Assert.Equal(50000000, preview.Sum(room => room.Lines.Single(line => line.Key == "rent").Amount));
        Assert.Equal(19, db.Contracts.Count()); Assert.Equal(12, db.ContractCoResidents.Count());
    }

    public void Dispose() => db.Dispose();
}
