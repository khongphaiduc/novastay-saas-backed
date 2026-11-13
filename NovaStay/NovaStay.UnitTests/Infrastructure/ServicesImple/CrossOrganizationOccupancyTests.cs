using Microsoft.EntityFrameworkCore;
using NovaStay.Application.Common;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

// Kiểm tra người ở được xét theo từng tổ chức: hợp đồng Active của chủ trọ khác không chặn xếp người ở ở đây.
public sealed class CrossOrganizationOccupancyTests
{
    private readonly HostContext db = new(new DbContextOptionsBuilder<HostContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Guid orgA = Guid.NewGuid(), orgB = Guid.NewGuid();
    private readonly Guid propertyA = Guid.NewGuid(), propertyB = Guid.NewGuid();
    private readonly Guid roomA = Guid.NewGuid(), roomB = Guid.NewGuid();
    private readonly Guid resident = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

    public CrossOrganizationOccupancyTests()
    {
        var account = new Account { Id = Guid.NewGuid(), AccountType = "Resident", CustomerName = "Test", Phone = "000", IsActive = true };
        var person = new Resident { Id = resident, AccountId = account.Id, FullName = "Test", Phone = "000", Sex = "Other" };
        db.AddRange(account, person);
        db.Organizations.AddRange(
            new Organization { Id = orgA, OwnerAccountId = Guid.NewGuid(), BusinessName = "A", BusinessArea = "A", OwnerEmail = "a@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Trial" },
            new Organization { Id = orgB, OwnerAccountId = Guid.NewGuid(), BusinessName = "B", BusinessArea = "B", OwnerEmail = "b@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Trial" });
        db.Properties.AddRange(
            new Property { Id = propertyA, OrganizationId = orgA, PropertyName = "A", Address = "A", PropertyType = "BoardingHouse", Status = "Active" },
            new Property { Id = propertyB, OrganizationId = orgB, PropertyName = "B", Address = "B", PropertyType = "BoardingHouse", Status = "Active" });
        db.Rooms.AddRange(
            new Room { Id = roomA, PropertyId = propertyA, RoomNumber = "3", MaxOccupants = 1, Status = "Available", BasePrice = 3000000, RowVersion = [] },
            new Room { Id = roomB, PropertyId = propertyB, RoomNumber = "7", MaxOccupants = 1, Status = "Occupied", BasePrice = 3000000, RowVersion = [] });
        db.ResidentMemberships.AddRange(
            new ResidentMembership { Id = Guid.NewGuid(), AccountId = account.Id, ResidentId = resident, OrganizationId = orgA, MembershipCode = "A1", Status = "ACTIVE" },
            new ResidentMembership { Id = Guid.NewGuid(), AccountId = account.Id, ResidentId = resident, OrganizationId = orgB, MembershipCode = "B1", Status = "ACTIVE" });
        // Hợp đồng Active của chủ trọ B: cư dân là đại diện
        db.Contracts.Add(new Contract
        {
            Id = Guid.NewGuid(), OrganizationId = orgB, PropertyId = propertyB, RoomId = roomB, ResidentId = resident,
            StartDate = Today.AddMonths(-1), EndDate = Today.AddYears(1), Status = "Active", CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    private Contract NewContractInA() => new()
    {
        Id = Guid.NewGuid(), OrganizationId = orgA, PropertyId = propertyA, RoomId = roomA, ResidentId = resident,
        StartDate = Today, EndDate = Today.AddMonths(6), Status = "Active", CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task ActiveContractWithAnotherOrganization_DoesNotBlockNewContract()
    {
        db.Contracts.Add(NewContractInA());

        await db.SaveChangesAsync();

        Assert.Equal(2, await db.Contracts.CountAsync(c => c.Status == "Active" && c.ResidentId == resident));
    }

    [Fact]
    public async Task ActiveContractInSameOrganization_StillBlocksNewContract()
    {
        var sameOrgOtherRoom = new Room { Id = Guid.NewGuid(), PropertyId = propertyA, RoomNumber = "5", MaxOccupants = 1, Status = "Occupied", BasePrice = 3000000, RowVersion = [] };
        db.Rooms.Add(sameOrgOtherRoom);
        db.Contracts.Add(new Contract
        {
            Id = Guid.NewGuid(), OrganizationId = orgA, PropertyId = propertyA, RoomId = sameOrgOtherRoom.Id, ResidentId = resident,
            StartDate = Today.AddMonths(-1), EndDate = Today.AddYears(1), Status = "Active", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        db.Contracts.Add(NewContractInA());

        await Assert.ThrowsAsync<RoomOccupancyException>(() => db.SaveChangesAsync());
    }
}
