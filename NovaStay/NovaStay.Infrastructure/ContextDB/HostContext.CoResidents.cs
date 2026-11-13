using NovaStay.Application.Common;
using System.Data;
using Microsoft.EntityFrameworkCore;
using NovaStay.Infrastructure.Models;

namespace NovaStay.Infrastructure.ContextDB;

public partial class HostContext
{
    public DbSet<ContractCoResident> ContractCoResidents => Set<ContractCoResident>();

    private static void ConfigureCoResidents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractCoResident>(entity =>
        {
            entity.HasKey(x => new { x.ContractId, x.ResidentId });
            entity.HasIndex(x => new { x.ResidentId, x.LeftAt });
            entity.Property(x => x.JoinedAt).HasColumnType("datetime2");
            entity.Property(x => x.LeftAt).HasColumnType("datetime2");
            entity.HasOne(x => x.Contract).WithMany(x => x.CoResidents).HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Resident).WithMany().HasForeignKey(x => x.ResidentId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        var relevant = ChangeTracker.Entries().Any(e =>
            e.Entity is ContractCoResident && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted ||
            e.Entity is Contract && (e.State == EntityState.Added || e.State == EntityState.Modified &&
                new[] { "ResidentId", "OrganizationId", "PropertyId", "RoomId", "Status", "StartDate", "EndDate" }.Any(p => e.Property(p).IsModified)) ||
            e.Entity is Room && e.State == EntityState.Modified && (e.Property("MaxOccupants").IsModified || e.Property("IsDeleted").IsModified) ||
            e.Entity is ResidentMembership && e.State is EntityState.Modified or EntityState.Deleted);
        if (!relevant) return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        // Keep capacity/membership checks and writes atomic, including legacy contract creation/renewal paths.
        await using var transaction = Database.IsRelational() && Database.CurrentTransaction == null
            ? await Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken) : null;
        await ValidateOccupancy(cancellationToken);
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task ValidateOccupancy(CancellationToken ct)
    {
        var changedContracts = ChangeTracker.Entries<Contract>().Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached).ToArray();
        var changedPeople = ChangeTracker.Entries<ContractCoResident>().Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached).ToArray();
        var changedRooms = ChangeTracker.Entries<Room>().Where(e => e.State == EntityState.Modified).ToArray();
        var changedMemberships = ChangeTracker.Entries<ResidentMembership>().Where(e => e.State is EntityState.Modified or EntityState.Deleted).ToArray();
        var ids = changedContracts.Select(e => e.Entity.Id).Concat(changedPeople.Select(e => e.Entity.ContractId)).ToHashSet();
        var rooms = changedRooms.Select(e => e.Entity.Id).ToArray();
        var people = changedMemberships.Select(e => e.Entity.ResidentId).ToArray();
        var representatives = changedContracts.Select(e => e.Entity.ResidentId).ToArray();
        // Kiểm tra người ở theo từng tổ chức: hợp đồng của chủ trọ khác không chặn việc xếp người ở ở đây.
        // Phòng luôn thuộc đúng một tổ chức nên kiểm tra theo phòng không cần lọc tổ chức.
        var personOrgs = changedContracts.Select(e => e.Entity.OrganizationId)
            .Concat(changedMemberships.Select(e => e.Entity.OrganizationId)).ToHashSet();
        if (changedPeople.Length > 0)
        {
            var personContractIds = changedPeople.Select(e => e.Entity.ContractId).Distinct().ToArray();
            personOrgs.UnionWith(await Contracts.AsNoTracking().Where(c => personContractIds.Contains(c.Id))
                .Select(c => c.OrganizationId).ToListAsync(ct));
        }
        var personOrgIds = personOrgs.ToArray();
        foreach (var id in await Contracts.Where(c => c.Status == "Active" &&
            (rooms.Contains(c.RoomId) ||
             (personOrgIds.Contains(c.OrganizationId) &&
              (people.Contains(c.ResidentId) || c.CoResidents.Any(p => p.LeftAt == null && people.Contains(p.ResidentId))))))
            .Select(c => c.Id).ToListAsync(ct)) ids.Add(id);
        if (ids.Count == 0) return;

        var contracts = (await Contracts.AsNoTracking().Where(c => ids.Contains(c.Id)).ToListAsync(ct)).ToDictionary(c => c.Id);
        foreach (var entry in changedContracts)
        {
            if (entry.State == EntityState.Deleted) contracts.Remove(entry.Entity.Id);
            else contracts[entry.Entity.Id] = entry.Entity;
        }
        var coResidents = (await ContractCoResidents.AsNoTracking().Where(p => ids.Contains(p.ContractId)).ToListAsync(ct))
            .ToDictionary(p => (p.ContractId, p.ResidentId));
        foreach (var entry in changedPeople)
        {
            var key = (entry.Entity.ContractId, entry.Entity.ResidentId);
            if (entry.State == EntityState.Deleted) coResidents.Remove(key); else coResidents[key] = entry.Entity;
        }
        var active = contracts.Values.Where(c => c.Status == "Active").ToArray();
        var residentIds = active.Select(c => c.ResidentId).Concat(coResidents.Values.Where(p => p.LeftAt == null &&
            active.Any(c => c.Id == p.ContractId)).Select(p => p.ResidentId)).Distinct().ToArray();
        var roomIds = active.Select(c => c.RoomId).Distinct().ToArray();
        var orgIds = active.Select(c => c.OrganizationId).Concat(personOrgIds).Distinct().ToArray();
        var existing = await Contracts.AsNoTracking().Where(c => c.Status == "Active" && orgIds.Contains(c.OrganizationId) &&
            (roomIds.Contains(c.RoomId) || residentIds.Contains(c.ResidentId) || c.CoResidents.Any(p => p.LeftAt == null && residentIds.Contains(p.ResidentId))))
            .Select(c => new { c.Id, c.RoomId, c.ResidentId, c.EndDate }).ToListAsync(ct);
        var outside = existing.Where(c => !ids.Contains(c.Id)).ToArray();
        var outsideIds = outside.Select(c => c.Id).ToArray();
        var outsidePeople = await ContractCoResidents.AsNoTracking().Where(p => outsideIds.Contains(p.ContractId) && p.LeftAt == null)
            .Select(p => new { p.ContractId, p.ResidentId }).ToListAsync(ct);
        // Cư dân đang bị giữ bởi một hợp đồng Active khác (đại diện hoặc người ở ghép) -> hợp đồng đó để báo lỗi cụ thể
        var occupiedBy = new Dictionary<Guid, Guid>();
        foreach (var c in outside) occupiedBy.TryAdd(c.ResidentId, c.Id);
        foreach (var p in outsidePeople) occupiedBy.TryAdd(p.ResidentId, p.ContractId);
        var occupied = new HashSet<Guid>(occupiedBy.Keys);
        var occupiedRooms = outside.Select(c => c.RoomId).ToHashSet();
        var outsideContracts = outside.ToDictionary(c => c.Id);
        var outsideRoomNumbers = outsideContracts.Count == 0 ? new Dictionary<Guid, string>()
            : await Rooms.AsNoTracking().Where(r => outsideContracts.Values.Select(c => c.RoomId).Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.RoomNumber, ct);
        var memberships = await ResidentMemberships.AsNoTracking().Where(m => residentIds.Contains(m.ResidentId)).ToListAsync(ct);
        foreach (var entry in changedMemberships)
        {
            memberships.RemoveAll(m => m.Id == entry.Entity.Id);
            if (entry.State != EntityState.Deleted) memberships.Add(entry.Entity);
        }
        foreach (var entry in ChangeTracker.Entries<ResidentMembership>().Where(e => e.State == EntityState.Added)) memberships.Add(entry.Entity);
        var validPeople = await Residents.Where(r => residentIds.Contains(r.Id) && r.Account.IsActive == true)
            .Select(r => r.Id).ToListAsync(ct);
        foreach (var resident in ChangeTracker.Entries<Resident>().Where(e => e.State == EntityState.Added))
        {
            var account = ChangeTracker.Entries<Account>().FirstOrDefault(e => e.Entity.Id == resident.Entity.AccountId)?.Entity;
            if (account?.IsActive == true || account == null && await Accounts.AnyAsync(a => a.Id == resident.Entity.AccountId && a.IsActive == true, ct))
                validPeople.Add(resident.Entity.Id);
        }
        foreach (var contract in active)
        {
            if (contract.EndDate <= contract.StartDate)
                throw new RoomOccupancyException("Ngày kết thúc phải sau ngày bắt đầu.");
            var room = changedRooms.FirstOrDefault(e => e.Entity.Id == contract.RoomId)?.Entity
                ?? ChangeTracker.Entries<Room>().FirstOrDefault(e => e.Entity.Id == contract.RoomId && e.State == EntityState.Added)?.Entity
                ?? await Rooms.AsNoTracking().SingleOrDefaultAsync(r => r.Id == contract.RoomId, ct);
            if (room == null || room.IsDeleted || room.PropertyId != contract.PropertyId ||
                !(ChangeTracker.Entries<Property>().Any(e => e.State == EntityState.Added && e.Entity.Id == contract.PropertyId && e.Entity.OrganizationId == contract.OrganizationId)
                    || await Properties.AnyAsync(p => p.Id == contract.PropertyId && p.OrganizationId == contract.OrganizationId && p.Status != "Deleted", ct)))
                throw new RoomOccupancyException("Phòng không thuộc đúng nhà trọ của hợp đồng.");
            if (!occupiedRooms.Add(contract.RoomId)) throw new RoomOccupancyException("Phòng đã có hợp đồng đang hiệu lực.");
            var members = coResidents.Values.Where(p => p.ContractId == contract.Id && p.LeftAt == null).Select(p => p.ResidentId).ToList();
            if (members.Contains(contract.ResidentId)) throw new RoomOccupancyException("Người đại diện không được thêm lại vào danh sách ở ghép.");
            members.Insert(0, contract.ResidentId);
            if (members.Count > (room.MaxOccupants ?? 1)) throw new RoomOccupancyException("Số người vượt quá sức chứa của phòng.");
            foreach (var id in members)
            {
                if (!validPeople.Contains(id) || !memberships.Any(m => m.ResidentId == id && m.OrganizationId == contract.OrganizationId &&
                    string.Equals(m.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)))
                    throw new RoomOccupancyException("Cư dân phải đang hoạt động và thuộc cùng nhà trọ.");
                if (!occupied.Add(id))
                {
                    if (occupiedBy.TryGetValue(id, out var holderId) && outsideContracts.TryGetValue(holderId, out var holder))
                    {
                        var roomNo = outsideRoomNumbers.TryGetValue(holder.RoomId, out var number) ? number : "khác";
                        throw new RoomOccupancyException(
                            $"Cư dân đang có hợp đồng hiệu lực ở phòng {roomNo} (kết thúc ngày {holder.EndDate:dd/MM/yyyy}). " +
                            "Hãy thanh lý hoặc kết thúc hợp đồng đó trước khi tạo hợp đồng mới.");
                    }
                    throw new RoomOccupancyException("Cư dân đang được xếp vào nhiều hợp đồng cùng lúc.");
                }
            }
        }
    }
}
