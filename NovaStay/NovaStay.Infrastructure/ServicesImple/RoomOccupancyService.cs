using NovaStay.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class RoomOccupancyService(HostContext db)
{
    private async Task<Contract> Contract(Guid owner, Guid id, CancellationToken ct) =>
        await db.Contracts.Include(c => c.Room).Include(c => c.Property).Include(c => c.Resident)
            .Include(c => c.CoResidents).ThenInclude(p => p.Resident)
            .SingleOrDefaultAsync(c => c.Id == id && c.Organization.OwnerAccountId == owner, ct)
        ?? throw new KeyNotFoundException("Không tìm thấy hợp đồng.");

    private static RoomOccupancyDto Map(Contract c) => new(c.Id, c.RoomId, c.Room.RoomNumber,
        c.PropertyId, c.Property.PropertyName, c.Room.MaxOccupants ?? 1,
        new[] { new RoomOccupantDto(c.ResidentId, c.Resident.FullName, c.Resident.Phone, true) }
            .Concat(c.CoResidents.Where(p => p.LeftAt == null).OrderBy(p => p.JoinedAt).ThenBy(p => p.ResidentId)
                .Select(p => new RoomOccupantDto(p.ResidentId, p.Resident.FullName, p.Resident.Phone, false))).ToArray());

    public async Task<RoomOccupancyDto> Get(Guid owner, Guid id, CancellationToken ct) => Map(await Contract(owner, id, ct));

    public async Task<IReadOnlyList<RoomOccupancyDto>> List(Guid owner, Guid organization, CancellationToken ct)
    {
        if (!await db.Organizations.AnyAsync(o => o.Id == organization && o.OwnerAccountId == owner, ct))
            throw new KeyNotFoundException("Không tìm thấy nhà trọ.");
        var rows = await db.Contracts.AsNoTracking().Include(c => c.Room).Include(c => c.Property).Include(c => c.Resident)
            .Include(c => c.CoResidents).ThenInclude(p => p.Resident).AsSplitQuery()
            .Where(c => c.OrganizationId == organization && c.Status == "Active" && !c.Room.IsDeleted)
            .OrderBy(c => c.Room.RoomNumber).ToListAsync(ct);
        return rows.Select(Map).ToArray();
    }

    public async Task<PagedResult<RoomOccupantDto>> Candidates(Guid owner, Guid id, string? search, int page, int size, CancellationToken ct)
    {
        if (page < 1 || size is < 1 or > 100) throw new ArgumentException("Phân trang không hợp lệ.");
        var contract = await Contract(owner, id, ct);
        var query = db.ResidentMemberships.AsNoTracking().Where(m => m.OrganizationId == contract.OrganizationId &&
            (m.Status == "ACTIVE" || m.Status == "Active") && m.Account.IsActive == true &&
            !db.Contracts.Any(c => c.ResidentId == m.ResidentId && c.Status == "Active") &&
            !db.ContractCoResidents.Any(p => p.ResidentId == m.ResidentId && p.LeftAt == null && p.Contract.Status == "Active"));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(m => m.Resident.FullName.Contains(search.Trim()) || m.Resident.Phone.Contains(search.Trim()));
        return new PagedResult<RoomOccupantDto>
        {
            PageIndex = page, PageSize = size, TotalCount = await query.CountAsync(ct),
            Items = await query.OrderBy(m => m.Resident.FullName).ThenBy(m => m.Id).Skip(checked((page - 1) * size)).Take(size)
                .Select(m => new RoomOccupantDto(m.ResidentId, m.Resident.FullName, m.Resident.Phone, false)).ToListAsync(ct)
        };
    }

    public async Task<RoomOccupancyDto> Set(Guid owner, Guid id, IReadOnlyCollection<Guid> residentIds, CancellationToken ct)
    {
        try { return await SetCore(owner, id, residentIds, ct); }
        catch (SqlException e) when (e.Number == 1205)
        { throw new RoomOccupancyException("Có thao tác xếp phòng đồng thời. Vui lòng tải lại và thử lại."); }
        catch (DbUpdateException e) when (e.InnerException is SqlException sql && sql.Number is 1205 or 2601 or 2627)
        { throw new RoomOccupancyException("Dữ liệu xếp phòng vừa thay đổi. Vui lòng tải lại và thử lại."); }
    }

    private async Task<RoomOccupancyDto> SetCore(Guid owner, Guid id, IReadOnlyCollection<Guid> residentIds, CancellationToken ct)
    {
        if (residentIds.Count > 50 || residentIds.Any(x => x == Guid.Empty) || residentIds.Distinct().Count() != residentIds.Count)
            throw new ArgumentException("Danh sách người ở ghép không hợp lệ hoặc bị trùng.");
        await using var transaction = db.Database.IsRelational() && db.Database.CurrentTransaction == null
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        if (db.Database.IsSqlServer())
        {
            // Serialize replacement of a contract's full list before reading it (not just before SaveChanges).
            var owned = await db.Contracts.FromSqlInterpolated($"SELECT * FROM dbo.Contracts WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}")
                .Where(c => c.Organization.OwnerAccountId == owner).Select(c => c.Id).SingleOrDefaultAsync(ct);
            if (owned == Guid.Empty) throw new KeyNotFoundException("Không tìm thấy hợp đồng.");
        }
        var contract = await Contract(owner, id, ct);
        if (contract.Status != "Active") throw new RoomOccupancyException("Chỉ sửa người ở ghép của hợp đồng đang hiệu lực.");
        if (residentIds.Contains(contract.ResidentId)) throw new RoomOccupancyException("Người đại diện không được thêm lại vào danh sách ở ghép.");
        if (residentIds.Count + 1 > (contract.Room.MaxOccupants ?? 1)) throw new RoomOccupancyException("Số người vượt quá sức chứa của phòng.");
        var now = DateTime.UtcNow;
        foreach (var person in contract.CoResidents)
        {
            if (!residentIds.Contains(person.ResidentId) && person.LeftAt == null) person.LeftAt = now;
            if (residentIds.Contains(person.ResidentId) && person.LeftAt != null) { person.LeftAt = null; person.JoinedAt = now; }
        }
        foreach (var resident in residentIds.Where(x => !contract.CoResidents.Any(p => p.ResidentId == x)))
            db.ContractCoResidents.Add(new ContractCoResident { ContractId = id, ResidentId = resident, JoinedAt = now });
        // HostContext performs membership, capacity and cross-room checks inside the SQL transaction.
        await db.SaveChangesAsync(ct);
        if (transaction != null) await transaction.CommitAsync(ct);
        db.ChangeTracker.Clear();
        return await Get(owner, id, ct);
    }
}
