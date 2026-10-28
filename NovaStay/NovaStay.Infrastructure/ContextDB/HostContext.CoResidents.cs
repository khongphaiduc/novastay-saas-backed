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
                new[] { "ResidentId", "OrganizationId", "PropertyId", "RoomId", "Status" }.Any(p => e.Property(p).IsModified)) ||
            e.Entity is Room && e.State == EntityState.Modified && e.Property("MaxOccupants").IsModified ||
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
        // Preserve legacy primary-only contracts. Enforce the shared-occupancy invariant whenever
        // a co-resident, their representative, their room, or their membership is involved.
        if (changedPeople.Length == 0 && !await ContractCoResidents.AnyAsync(p => p.LeftAt == null &&
            (ids.Contains(p.ContractId) || p.Contract.Status == "Active" && (representatives.Contains(p.ResidentId) || rooms.Contains(p.Contract.RoomId) ||
                people.Contains(p.ResidentId) || people.Contains(p.Contract.ResidentId))), ct)) return;
        foreach (var id in await Contracts.Where(c => c.Status == "Active" &&
            (rooms.Contains(c.RoomId) || people.Contains(c.ResidentId) || c.CoResidents.Any(p => p.LeftAt == null && people.Contains(p.ResidentId))))
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
        var existing = await Contracts.AsNoTracking().Where(c => c.Status == "Active" &&
            (roomIds.Contains(c.RoomId) || residentIds.Contains(c.ResidentId) || c.CoResidents.Any(p => p.LeftAt == null && residentIds.Contains(p.ResidentId))))
            .Select(c => new { c.Id, c.RoomId, c.ResidentId }).ToListAsync(ct);
        var outside = existing.Where(c => !ids.Contains(c.Id)).ToArray();
        var outsideIds = outside.Select(c => c.Id).ToArray();
        var outsidePeople = await ContractCoResidents.AsNoTracking().Where(p => outsideIds.Contains(p.ContractId) && p.LeftAt == null)
            .Select(p => p.ResidentId).ToListAsync(ct);
        var occupied = outside.Select(c => c.ResidentId).Concat(outsidePeople).ToHashSet();
        var occupiedRooms = outside.Select(c => c.RoomId).ToHashSet();
        var memberships = await ResidentMemberships.AsNoTracking().Where(m => residentIds.Contains(m.ResidentId)).ToListAsync(ct);
        foreach (var entry in changedMemberships)
        {
            memberships.RemoveAll(m => m.Id == entry.Entity.Id);
            if (entry.State != EntityState.Deleted) memberships.Add(entry.Entity);
        }
        foreach (var entry in ChangeTracker.Entries<ResidentMembership>().Where(e => e.State == EntityState.Added)) memberships.Add(entry.Entity);
        var validPeople = await Residents.Where(r => residentIds.Contains(r.Id) && r.Account.IsActive == true)
            .Select(r => r.Id).ToListAsync(ct);
        foreach (var resident in ChangeTracker.Entries<Resident>().Where(e => e.State == EntityState.Added)) validPeople.Add(resident.Entity.Id);
        foreach (var contract in active)
        {
            var room = changedRooms.FirstOrDefault(e => e.Entity.Id == contract.RoomId)?.Entity
                ?? ChangeTracker.Entries<Room>().FirstOrDefault(e => e.Entity.Id == contract.RoomId && e.State == EntityState.Added)?.Entity
                ?? await Rooms.AsNoTracking().SingleOrDefaultAsync(r => r.Id == contract.RoomId, ct);
            if (room == null || room.IsDeleted || room.PropertyId != contract.PropertyId ||
                !await Properties.AnyAsync(p => p.Id == contract.PropertyId && p.OrganizationId == contract.OrganizationId, ct))
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
                if (!occupied.Add(id)) throw new RoomOccupancyException("Cư dân đã được xếp vào phòng khác.");
            }
        }
    }
}
