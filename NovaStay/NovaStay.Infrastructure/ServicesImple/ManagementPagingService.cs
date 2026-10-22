using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Repositories;

namespace NovaStay.Infrastructure.ServicesImple;

/// <summary>Database pagination for management screens; legacy array endpoints remain compatible.</summary>
public sealed class ManagementPagingService(HostContext db)
{
    private async Task Property(Guid owner, Guid propertyId, Guid? organizationId, CancellationToken ct)
    {
        if (!await db.Properties.AnyAsync(p => p.Id == propertyId && p.Organization.OwnerAccountId == owner &&
                (!organizationId.HasValue || p.OrganizationId == organizationId), ct))
            throw new KeyNotFoundException("Không tìm thấy nhà trọ.");
    }

    public async Task<PagedResult<IncomeReceiptDto>> Income(Guid owner, Guid org, Guid property, int page, int size,
        ApprovalStatus? status, PaymentMethod? method, string? search, CancellationToken ct)
    {
        Pagination.Offset(page, size);
        await Property(owner, property, org, ct);
        var query = db.IncomeReceipts.AsNoTracking().Where(x => x.OrganizationId == org && x.PropertyId == property);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value.ToString());
        if (method.HasValue) query = query.Where(x => x.PaymentMethod == method.Value.ToString());
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ReceiptNumber.Contains(term) || x.PayerName.Contains(term) || (x.ReferenceCode != null && x.ReferenceCode.Contains(term)));
        }
        return await Page(query.Include(x => x.IncomeCategory).Include(x => x.Room).Include(x => x.Resident)
            .Include(x => x.CollectedByStaffUser).OrderByDescending(x => x.CollectedAt).ThenByDescending(x => x.CreatedAt).ThenBy(x => x.Id),
            IncomeReceiptRepository.MapToDto, page, size, ct);
    }

    public async Task<PagedResult<ExpenseDto>> Expenses(Guid owner, Guid org, Guid property, int page, int size,
        ApprovalStatus? status, PaymentMethod? method, string? search, CancellationToken ct)
    {
        Pagination.Offset(page, size);
        await Property(owner, property, org, ct);
        var query = db.Expenses.AsNoTracking().Where(x => x.OrganizationId == org && x.PropertyId == property);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value.ToString());
        if (method.HasValue) query = query.Where(x => x.PaymentMethod == method.Value.ToString());
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ExpenseNumber.Contains(term) || x.PayeeName.Contains(term) || (x.ReferenceCode != null && x.ReferenceCode.Contains(term)));
        }
        return await Page(query.Include(x => x.ExpenseCategory).Include(x => x.Room).Include(x => x.ApprovedByStaffUser)
            .Include(x => x.CreatedByStaffUser).OrderByDescending(x => x.SpentAt).ThenByDescending(x => x.CreatedAt).ThenBy(x => x.Id),
            ExpenseRepository.MapToDto, page, size, ct);
    }

    public async Task<PagedResult<UtilityMeterDto>> Meters(Guid owner, Guid? propertyId, Guid? roomId, int page, int size,
        string? search, CancellationToken ct)
    {
        Pagination.Offset(page, size);
        if (roomId.HasValue)
        {
            propertyId = await db.Rooms.Where(x => x.Id == roomId && !x.IsDeleted && x.Property.Organization.OwnerAccountId == owner)
                .Select(x => (Guid?)x.PropertyId).SingleOrDefaultAsync(ct);
        }
        if (!propertyId.HasValue) throw new KeyNotFoundException("Không tìm thấy phòng.");
        await Property(owner, propertyId.Value, null, ct);
        var query = db.UtilityMeters.AsNoTracking().Where(x => x.Room.PropertyId == propertyId && !x.Room.IsDeleted && x.IsActive);
        if (roomId.HasValue) query = query.Where(x => x.RoomId == roomId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Room.RoomNumber.Contains(term) || x.PropertyService.ServiceName.Contains(term) || (x.MeterCode != null && x.MeterCode.Contains(term)));
        }
        // The page contains only the latest reading per meter; history has its own paged endpoint.
        return await Page(query.Include(x => x.PropertyService)
            .Include(x => x.UtilityReadings.OrderByDescending(r => r.BillingPeriod).ThenByDescending(r => r.Id).Take(1))
            .AsSplitQuery().OrderBy(x => x.Room.RoomNumber).ThenBy(x => x.PropertyService.ServiceName).ThenBy(x => x.Id),
            UtilityService.MapMeter, page, size, ct);
    }

    public async Task<PagedResult<UtilityReadingDto>> Readings(Guid owner, Guid meter, int page, int size, string? period, CancellationToken ct)
    {
        Pagination.Offset(page, size);
        if (!await db.UtilityMeters.AnyAsync(x => x.Id == meter && x.Room.Property.Organization.OwnerAccountId == owner, ct))
            throw new KeyNotFoundException("Không tìm thấy đồng hồ.");
        var query = db.UtilityReadings.AsNoTracking().Where(x => x.UtilityMeterId == meter);
        if (!string.IsNullOrWhiteSpace(period))
        {
            BillingService.ParsePeriod(period);
            query = query.Where(x => x.BillingPeriod == period);
        }
        return await Page(query.OrderByDescending(x => x.BillingPeriod).ThenByDescending(x => x.ReadingDate).ThenBy(x => x.Id),
            UtilityService.MapReading, page, size, ct);
    }

    private static async Task<PagedResult<TDto>> Page<T, TDto>(IQueryable<T> query, Func<T, TDto> map, int page, int size, CancellationToken ct)
    {
        var offset = Pagination.Offset(page, size);
        var count = await query.CountAsync(ct);
        var items = await query.Skip(offset).Take(size).ToListAsync(ct);
        return new() { Items = items.Select(map).ToList(), TotalCount = count, PageIndex = page, PageSize = size };
    }
}
