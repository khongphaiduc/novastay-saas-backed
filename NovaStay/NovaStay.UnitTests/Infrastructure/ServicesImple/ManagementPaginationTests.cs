using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed partial class BillingServiceTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 1001)]
    [InlineData(int.MaxValue, 1000)]
    public void Pagination_RejectsInvalidBoundsAndOverflow(int page, int size) =>
        Assert.Throws<ArgumentException>(() => Pagination.Offset(page, size));

    [Fact]
    public async Task PagedBills_StablePagesCorrectTotalsAndFilters()
    {
        await SaveBank();
        foreach (var period in new[] { "2026-10", "2026-11", "2026-12" })
        {
            var request = Request("rent"); request.Period = period; request.DueDate = BillingService.ParsePeriod(period).AddDays(20);
            await service.Issue(owner, request);
        }
        foreach (var invoice in db.Invoices) invoice.CreatedAt = new DateTime(2026, 10, 1);
        await db.SaveChangesAsync();
        var first = await service.OwnerBillsPaged(owner, property.Id, 1, 2, null, null, null);
        var last = await service.OwnerBillsPaged(owner, property.Id, 2, 2, null, null, null);
        Assert.Equal(3, first.TotalCount); Assert.Equal(2, first.TotalPages);
        Assert.True(first.HasNextPage); Assert.False(first.HasPreviousPage);
        Assert.True(last.HasPreviousPage); Assert.False(last.HasNextPage);
        Assert.Equal(2, first.Items.Count); Assert.Single(last.Items);
        Assert.Equal(3, first.Items.Concat(last.Items).Select(x => x.Id).Distinct().Count());
        Assert.Equal(first.Items.Select(x => x.Id), (await service.OwnerBillsPaged(owner, property.Id, 1, 2, null, null, null)).Items.Select(x => x.Id));
        var empty = await service.OwnerBillsPaged(owner, property.Id, 3, 2, null, null, null);
        Assert.Empty(empty.Items); Assert.Equal(3, empty.TotalCount);
        await service.Report(residentAccount, first.Items[0].Id);
        var filtered = await service.OwnerBillsPaged(owner, property.Id, 1, 1, "AwaitingConfirmation", null, "101");
        Assert.Equal(1, filtered.TotalCount); Assert.Single(filtered.Items);
        var periodPage = await service.OwnerBillsPaged(owner, property.Id, 1, 10, null, "2026-11", null);
        Assert.Single(periodPage.Items); Assert.Equal(1, periodPage.TotalCount);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.OwnerBillsPaged(owner, Guid.NewGuid(), 1, 10, null, null, null));
    }

    [Fact]
    public async Task PagedIncomeAndExpenses_FilterBeforeCounting_AndRejectOtherOwner()
    {
        var incomeCategory = new NovaStay.Infrastructure.Models.IncomeCategory { Id = Guid.NewGuid(), OrganizationId = org, CategoryCode = "TEST", CategoryName = "Test", IsActive = true };
        var expenseCategory = new NovaStay.Infrastructure.Models.ExpenseCategory { Id = Guid.NewGuid(), OrganizationId = org, CategoryCode = "TEST", CategoryName = "Test", IsActive = true };
        db.AddRange(incomeCategory, expenseCategory);
        for (var i = 0; i < 5; i++)
        {
            db.IncomeReceipts.Add(new IncomeReceipt
            {
                Id = Guid.NewGuid(), OrganizationId = org, PropertyId = property.Id, IncomeCategoryId = incomeCategory.Id,
                ReceiptNumber = "IN" + i, IncomeType = "Other", PayerName = "Test", Amount = 100,
                Status = i < 3 ? "Approved" : "Pending", PaymentMethod = "BankTransfer", CollectedAt = new(2026, 10, 1)
            });
            db.Expenses.Add(new Expense
            {
                Id = Guid.NewGuid(), OrganizationId = org, PropertyId = property.Id, ExpenseCategoryId = expenseCategory.Id,
                ExpenseNumber = "EX" + i, ExpenseType = "Other", PayeeName = "Test", Amount = 100,
                Status = i < 3 ? "Approved" : "Pending", PaymentMethod = "BankTransfer", SpentAt = new(2026, 10, 1)
            });
        }
        await db.SaveChangesAsync();
        var paging = new ManagementPagingService(db);
        var income = await paging.Income(owner, org, property.Id, 1, 2, ApprovalStatus.Approved, PaymentMethod.BankTransfer, "IN", default);
        var expenses = await paging.Expenses(owner, org, property.Id, 2, 2, ApprovalStatus.Approved, PaymentMethod.BankTransfer, "EX", default);
        Assert.Equal(3, income.TotalCount); Assert.Equal(2, income.Items.Count);
        Assert.Equal(3, expenses.TotalCount); Assert.Single(expenses.Items);
        var empty = await paging.Income(owner, org, property.Id, 1, 2, null, null, "absent", default);
        Assert.Empty(empty.Items); Assert.Equal(0, empty.TotalCount); Assert.Equal(0, empty.TotalPages);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => paging.Income(Guid.NewGuid(), org, property.Id, 1, 10, null, null, null, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => paging.Expenses(owner, Guid.NewGuid(), property.Id, 1, 10, null, null, null, default));
    }

    [Fact]
    public async Task PagedMeters_OnlyLatestReadingIncluded_HistoryIsPagedIndependently()
    {
        var meter = await db.UtilityMeters.SingleAsync();
        for (var i = 1; i <= 5; i++)
            db.UtilityReadings.Add(new UtilityReading
            {
                Id = Guid.NewGuid(), UtilityMeterId = meter.Id, BillingPeriod = $"2026-{i:00}", ReadingDate = new(2026, i, 1),
                PreviousReading = i, CurrentReading = i + 1, Consumption = 1, UnitPrice = 3200, Amount = 3200, Status = "Recorded"
            });
        await db.SaveChangesAsync();
        var paging = new ManagementPagingService(db);
        var meters = await paging.Meters(owner, property.Id, null, 1, 10, "101", default);
        var item = Assert.Single(meters.Items);
        Assert.Equal(1, meters.TotalCount); Assert.Single(item.Readings);
        Assert.Equal("2026-10", item.LatestReading!.BillingPeriod);
        var first = await paging.Readings(owner, meter.Id, 1, 2, null, default);
        var next = await paging.Readings(owner, meter.Id, 2, 2, null, default);
        Assert.Equal(6, first.TotalCount); Assert.Equal(2, first.Items.Count);
        Assert.Empty(first.Items.Select(x => x.Id).Intersect(next.Items.Select(x => x.Id)));
        Assert.Single((await paging.Readings(owner, meter.Id, 1, 10, "2026-03", default)).Items);
        Assert.Single((await paging.Meters(owner, null, room.Id, 1, 10, null, default)).Items);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => paging.Meters(Guid.NewGuid(), property.Id, null, 1, 10, null, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => paging.Readings(Guid.NewGuid(), meter.Id, 1, 10, null, default));
    }
}
