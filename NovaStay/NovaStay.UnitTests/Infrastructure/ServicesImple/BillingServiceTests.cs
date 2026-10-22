using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.ServicesImple;
using PropertyService = NovaStay.Infrastructure.Models.PropertyService;
using RoomService = NovaStay.Infrastructure.Models.RoomService;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed partial class BillingServiceTests : IDisposable
{
    private readonly HostContext db;
    private readonly BillingService service;
    private readonly Guid owner = Guid.NewGuid();
    private readonly Guid residentAccount = Guid.NewGuid();
    private readonly Guid org = Guid.NewGuid();
    private readonly Property property;
    private readonly Room room;
    private readonly Resident resident;
    private readonly PropertyService electric;
    private readonly PropertyService internet;
    private static readonly byte[] PngHeader = [137, 80, 78, 71, 13, 10, 26, 10];

    public BillingServiceTests()
    {
        db = new(new DbContextOptionsBuilder<HostContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        service = new(db);
        db.Organizations.Add(new Organization { Id = org, OwnerAccountId = owner, BusinessName = "Test", BusinessArea = "Test", OwnerEmail = "test@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Active" });
        property = new() { Id = Guid.NewGuid(), OrganizationId = org, PropertyName = "Nhà A", Address = "Test", PropertyType = "BoardingHouse", Status = "Active" };
        room = new() { Id = Guid.NewGuid(), PropertyId = property.Id, RoomNumber = "101", BasePrice = 2500000, Status = "Occupied", RowVersion = [] };
        resident = new() { Id = Guid.NewGuid(), AccountId = residentAccount, FullName = "Người đại diện", Sex = "Other", Phone = "000" };
        db.AddRange(property, room, resident, new Contract { Id = Guid.NewGuid(), OrganizationId = org, PropertyId = property.Id, RoomId = room.Id, ResidentId = resident.Id, StartDate = new(2026, 1, 1), EndDate = new(2027, 1, 1), Status = "Active" });
        electric = new() { Id = Guid.NewGuid(), PropertyId = property.Id, ServiceName = "Điện", DefaultPrice = 3000, Unit = "kWh", IsActive = true };
        internet = new() { Id = Guid.NewGuid(), PropertyId = property.Id, ServiceName = "Internet", DefaultPrice = 150000, Unit = "phòng/tháng", BillingCycle = "Monthly", IsActive = true };
        var meter = new UtilityMeter { Id = Guid.NewGuid(), RoomId = room.Id, PropertyServiceId = electric.Id, MeterType = "Electric", Unit = "kWh", IsActive = true };
        db.AddRange(electric, internet, meter,
            new RoomService { Id = Guid.NewGuid(), RoomId = room.Id, PropertyServiceId = internet.Id, PriceOverride = 100000, IsActive = true },
            new UtilityReading { Id = Guid.NewGuid(), UtilityMeterId = meter.Id, BillingPeriod = "2026-10", ReadingDate = new(2026, 10, 20), PreviousReading = 100, CurrentReading = 125.5m, Consumption = 25.5m, UnitPrice = 3000, Amount = 76500, Status = "Recorded" });
        db.SaveChanges();
    }

    private IssueBillsRequest Request(params string[] keys) => new()
    {
        PropertyId = property.Id, Period = "2026-10", DueDate = new(2026, 10, 31), RoomIds = [room.Id], ChargeKeys = keys.ToList()
    };
    private Task<BankAccountDto> SaveBank(string number = "12345678", byte[]? image = null) =>
        service.SaveBank(owner, "Ngân hàng thử nghiệm", number, "CHU TRO", image, image == null ? null : "image/png", false);

    [Fact]
    public async Task SaveBank_NormalizesPastedWhitespaceAndPreservesLeadingZeroes()
    {
        var saved = await SaveBank(" 0012 3456\u00a07890\n");
        Assert.Equal("001234567890", saved.AccountNumber);
        Assert.Equal("001234567890", (await service.GetBank(owner))!.AccountNumber);
        var error = await Assert.ThrowsAsync<ArgumentException>(() => SaveBank("0012/3456"));
        Assert.StartsWith("Số tài khoản", error.Message);
        Assert.Single(db.BillingBankAccounts);
    }

    [Fact]
    public async Task ElectricCharge_UsesConfiguredPriceForUnbilledUsage_ThenFreezesIssuedInvoice()
    {
        await SaveBank();
        var reading = await db.UtilityReadings.SingleAsync();
        reading.CurrentReading = 120; reading.Consumption = 20; reading.UnitPrice = 0; reading.Amount = 0;
        electric.DefaultPrice = 3200;
        await db.SaveChangesAsync();
        var line = Assert.Single((await service.Preview(owner, property.Id, "2026-10")).Single().Lines, x => x.Key == $"service:{electric.Id}");
        Assert.Equal(20, line.Quantity);
        Assert.Equal(3200, line.UnitPrice);
        Assert.Equal(64000, line.Amount);
        var bill = Assert.Single(await service.Issue(owner, Request($"service:{electric.Id}")));
        electric.DefaultPrice = 4000;
        await db.SaveChangesAsync();
        Assert.Equal(64000, (await service.ResidentBills(residentAccount)).Single().TotalAmount);
        Assert.Equal(3200, bill.Lines.Single().UnitPrice);
        Assert.Equal(0, reading.Amount); // Historical reading itself was not rewritten.
    }

    [Fact]
    public async Task FullWorkflow_OnlyOwnerConfirmationCreatesIncome_AndCannotCollectTwice()
    {
        await SaveBank(image: PngHeader);
        var invoice = Assert.Single(await service.Issue(owner, Request("rent", $"service:{electric.Id}", $"service:{internet.Id}")));
        Assert.Equal(2676500, invoice.TotalAmount);
        Assert.Equal(3, invoice.Lines.Count);
        Assert.Equal("Unpaid", invoice.Status);
        Assert.Empty(db.IncomeReceipts);
        Assert.Empty(db.PaymentReceipts);
        Assert.Single(await service.ResidentBills(residentAccount));
        var reported = await service.Report(residentAccount, invoice.Id);
        Assert.Equal("AwaitingConfirmation", reported.Status);
        Assert.NotNull(reported.ReportedAt);
        Assert.Empty(db.IncomeReceipts);
        await service.Report(residentAccount, invoice.Id);
        var paid = await service.Confirm(owner, invoice.Id, "Đã đối chiếu ngân hàng");
        Assert.Equal("Paid", paid.Status);
        Assert.NotNull(paid.PaidAt);
        Assert.Equal(owner, (await db.Invoices.SingleAsync()).ConfirmedByAccountId);
        await service.Confirm(owner, invoice.Id, "Lặp lại");
        await service.Report(residentAccount, invoice.Id);
        Assert.Single(db.PaymentReceipts);
        Assert.Single(db.IncomeReceipts);
        Assert.Equal(invoice.TotalAmount, (await db.IncomeReceipts.SingleAsync()).Amount);
        Assert.Equal("Approved", (await db.IncomeReceipts.SingleAsync()).Status);
        Assert.Equal("Paid", (await service.ResidentBills(residentAccount)).Single().Status);
    }

    [Fact]
    public async Task Issue_UsesPriceSnapshot_AndPreventsDuplicateChargesButAllowsRemainingServices()
    {
        await SaveBank();
        var first = Assert.Single(await service.Issue(owner, Request("rent")));
        room.BasePrice = 4000000;
        await db.SaveChangesAsync();
        Assert.Equal(2500000, (await service.OwnerBills(owner)).Single().TotalAmount);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Issue(owner, Request("rent")));
        await service.Issue(owner, Request($"service:{internet.Id}"));
        Assert.Equal(2, await db.Invoices.CountAsync());
        Assert.Equal(2500000, first.TotalAmount);
        Assert.DoesNotContain((await service.Preview(owner, property.Id, "2026-10")).Single().Lines, x => x.Key == "rent");
    }

    [Fact]
    public async Task Permissions_DifferentOwnersAndResidentsCannotReadOrMutateBillsOrQr()
    {
        await SaveBank(image: PngHeader);
        var invoice = Assert.Single(await service.Issue(owner, Request("rent")));
        var stranger = Guid.NewGuid();
        db.Organizations.Add(new Organization { Id = Guid.NewGuid(), OwnerAccountId = stranger, BusinessName = "B", BusinessArea = "B", OwnerEmail = "b@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Active" });
        await db.SaveChangesAsync();
        Assert.Empty(await service.OwnerBills(stranger));
        Assert.Empty(await service.ResidentBills(stranger));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Confirm(stranger, invoice.Id, null));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Report(stranger, invoice.Id));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Qr(stranger, invoice.BankAccount!.Id));
        Assert.Equal(PngHeader, (await service.Qr(residentAccount, invoice.BankAccount!.Id)).Image);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Preview(stranger, property.Id, "2026-10"));
    }

    [Fact]
    public async Task BankVersion_ChangingDestinationDoesNotReuseOldQr_AndIssuedBillRetainsOriginal()
    {
        var original = await SaveBank(image: PngHeader);
        var bill = Assert.Single(await service.Issue(owner, Request("rent")));
        var changed = await SaveBank("87654321");
        Assert.False(changed.HasQr);
        Assert.Equal(original.Id, (await service.OwnerBills(owner)).Single().BankAccount!.Id);
        Assert.Equal("12345678", bill.BankAccount!.AccountNumber);
        Assert.Equal(PngHeader, (await service.Qr(residentAccount, original.Id)).Image);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Qr(residentAccount, changed.Id));
    }

    [Fact]
    public async Task MissingReading_BlocksSelectedUtility_ButRentCanStillBeIssued()
    {
        await SaveBank();
        db.UtilityReadings.RemoveRange(db.UtilityReadings);
        await db.SaveChangesAsync();
        var preview = Assert.Single(await service.Preview(owner, property.Id, "2026-10"));
        Assert.Contains(preview.Errors, e => e.Contains("chưa ghi đủ"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Issue(owner, Request("rent", $"service:{electric.Id}")));
        Assert.Empty(db.Invoices);
        await service.Issue(owner, Request("rent"));
        Assert.Single(db.Invoices);
    }

    [Fact]
    public async Task BulkIssue_CreatesSeparateBillsAndPayers_WithoutPartialWritesOnInvalidRoom()
    {
        await SaveBank();
        var otherRoom = new Room { Id = Guid.NewGuid(), PropertyId = property.Id, RoomNumber = "102", BasePrice = 3000000, Status = "Occupied", RowVersion = [] };
        var otherResident = new Resident { Id = Guid.NewGuid(), AccountId = Guid.NewGuid(), FullName = "Người nộp thứ hai", Sex = "Other", Phone = "000" };
        db.AddRange(otherRoom, otherResident, new Contract { Id = Guid.NewGuid(), RoomId = otherRoom.Id, PropertyId = property.Id, OrganizationId = org, ResidentId = otherResident.Id, Status = "Active", StartDate = new(2026, 1, 1), EndDate = new(2027, 1, 1) });
        await db.SaveChangesAsync();
        var invalid = Request("rent"); invalid.RoomIds.Add(Guid.NewGuid());
        await Assert.ThrowsAsync<ArgumentException>(() => service.Issue(owner, invalid));
        Assert.Empty(db.Invoices);
        var request = Request("rent"); request.RoomIds.Add(otherRoom.Id);
        var bills = await service.Issue(owner, request);
        Assert.Equal(2, bills.Count);
        Assert.Equal(2, bills.Select(x => x.Number).Distinct().Count());
        Assert.Equal(2500000, Assert.Single(await service.ResidentBills(residentAccount)).TotalAmount);
        Assert.Equal(3000000, Assert.Single(await service.ResidentBills(otherResident.AccountId)).TotalAmount);
    }

    [Fact]
    public async Task AmbiguousRepresentative_BlocksIssuingInsteadOfChargingTwice()
    {
        await SaveBank();
        db.Contracts.Add(new Contract { Id = Guid.NewGuid(), RoomId = room.Id, PropertyId = property.Id, OrganizationId = org, ResidentId = resident.Id, Status = "Active", StartDate = new(2026, 1, 1), EndDate = new(2027, 1, 1) });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Issue(owner, Request("rent")));
        Assert.Empty(db.Invoices);
    }

    [Fact]
    public async Task OwnerCanConfirmDirectly_ButBankConfigurationIsRequiredToIssue()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Issue(owner, Request("rent")));
        await SaveBank();
        var invoice = Assert.Single(await service.Issue(owner, Request("rent")));
        await service.Confirm(owner, invoice.Id, null);
        Assert.Single(db.PaymentReceipts);
        Assert.Null((await db.Invoices.SingleAsync()).TransferReportedAt);
    }

    [Theory]
    [InlineData("2026-1")]
    [InlineData("2026-13")]
    [InlineData("")]
    public void InvalidPeriod_IsRejected(string period) => Assert.Throws<ArgumentException>(() => BillingService.ParsePeriod(period));

    [Fact]
    public void InvalidQr_IsRejectedByFileSignatureAndSize()
    {
        Assert.Throws<ArgumentException>(() => BillingService.ValidateQr("<svg></svg>"u8.ToArray(), "image/png"));
        Assert.Throws<ArgumentException>(() => BillingService.ValidateQr(PngHeader, "image/svg+xml"));
        Assert.Throws<ArgumentException>(() => BillingService.ValidateQr(new byte[2 * 1024 * 1024 + 1], "image/png"));
    }

    [Fact]
    public void DatabaseModel_ProtectsConcurrentChargesAndSettlement_AndKeepsMeterPrecision()
    {
        // Build SQL Server metadata only; no connection is opened.
        using var sql = new HostContext(new DbContextOptionsBuilder<HostContext>()
            .UseSqlServer("Server=localhost;Database=BillingModelCheck;Integrated Security=true;TrustServerCertificate=true").Options);
        var line = sql.Model.FindEntityType(typeof(InvoiceLine))!;
        Assert.Equal("decimal(18,3)", line.FindProperty(nameof(InvoiceLine.Quantity))!.GetColumnType()!.Replace(" ", ""));
        Assert.Contains(line.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(
            new[] { "BillingOrganizationId", "BillingRoomId", "BillingPeriod", "SourceType", "SourceId" }));
        Assert.Contains(sql.Model.FindEntityType(typeof(PaymentReceipt))!.GetIndexes(),
            i => i.IsUnique && i.Properties.Count == 1 && i.Properties[0].Name == "InvoiceId");
        Assert.True(sql.Model.FindEntityType(typeof(Invoice))!.FindProperty(nameof(Invoice.BillingVersion))!.IsConcurrencyToken);
    }

    public void Dispose() => db.Dispose();
}
