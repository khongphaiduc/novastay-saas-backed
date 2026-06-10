using System;
using System.Collections.Generic;
using Host.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Host.Infrastructure.ContextDB;

public partial class HostContext : DbContext
{
    public HostContext()
    {
    }

    public HostContext(DbContextOptions<HostContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<AssetAssignment> AssetAssignments { get; set; }

    public virtual DbSet<Bed> Beds { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Broker> Brokers { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ListingImage> ListingImages { get; set; }

    public virtual DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<Resident> Residents { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomAvailability> RoomAvailabilities { get; set; }

    public virtual DbSet<RoomImage> RoomImages { get; set; }

    public virtual DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }

    public virtual DbSet<Technician> Technicians { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assets__3214EC071B1A8B14");

            entity.Property(e => e.AssetCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AssetName).HasMaxLength(100);
            entity.Property(e => e.BaseValue)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Assets)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assets__TenantId__02FC7413");
        });

        modelBuilder.Entity<AssetAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetAss__3214EC073B842B58");

            entity.HasIndex(e => new { e.RoomId, e.BedId }, "IX_AssetAssignments_RoomId_BedId");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Good");

            entity.HasOne(d => d.Asset).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK__AssetAssi__Asset__08B54D69");

            entity.HasOne(d => d.Bed).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.BedId)
                .HasConstraintName("FK__AssetAssi__BedId__0A9D95DB");

            entity.HasOne(d => d.Room).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__AssetAssi__RoomI__09A971A2");

            entity.HasOne(d => d.Tenant).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AssetAssi__Tenan__07C12930");
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Beds__3214EC07D3D03E54");

            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BedNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CardToken)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LockerId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Room).WithMany(p => p.Beds)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Beds__RoomId__7A672E12");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bookings__3214EC07E0378A72");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BookingType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GuestEmail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GuestName).HasMaxLength(100);
            entity.Property(e => e.GuestPhone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PaymentTransactionId)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Bed).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BedId)
                .HasConstraintName("FK__Bookings__BedId__1EA48E88");

            entity.HasOne(d => d.Property).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__Proper__1CBC4616");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__RoomId__1DB06A4F");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__Tenant__1BC821DD");
        });

        modelBuilder.Entity<Broker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Brokers__3214EC07D3AA9FC2");

            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TotalCommissionEarned)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.WalletBalance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Brokers)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Brokers__TenantI__2739D489");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contract__3214EC0707402DA0");

            entity.Property(e => e.BrokerCommission)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CommissionStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ContractPdfUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Bed).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.BedId)
                .HasConstraintName("FK__Contracts__BedId__2FCF1A8A");

            entity.HasOne(d => d.Booking).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Contracts__Booki__32AB8735");

            entity.HasOne(d => d.Broker).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.BrokerId)
                .HasConstraintName("FK__Contracts__Broke__31B762FC");

            entity.HasOne(d => d.Property).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__Prope__2DE6D218");

            entity.HasOne(d => d.Resident).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ResidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__Resid__30C33EC3");

            entity.HasOne(d => d.Room).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__RoomI__2EDAF651");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__Tenan__2CF2ADDF");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invoices__3214EC07023BFFC1");

            entity.HasIndex(e => new { e.Status, e.InvoicePeriod }, "IX_Invoices_Status_Period");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InvoicePeriod)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.QrCodeUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RoomPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServicesPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Booking).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Invoices__Bookin__395884C4");

            entity.HasOne(d => d.Contract).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK__Invoices__Contra__3864608B");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Tenant__37703C52");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Listings__3214EC0739EB0492");

            entity.HasIndex(e => e.IsPublished, "IX_Listings_IsPublished");

            entity.Property(e => e.Amenities).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Bed).WithMany(p => p.Listings)
                .HasForeignKey(d => d.BedId)
                .HasConstraintName("FK__Listings__BedId__123EB7A3");

            entity.HasOne(d => d.Property).WithMany(p => p.Listings)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Listings__Proper__10566F31");

            entity.HasOne(d => d.Room).WithMany(p => p.Listings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Listings__RoomId__114A936A");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Listings)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Listings__Tenant__0F624AF8");
        });

        modelBuilder.Entity<ListingImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ListingI__3214EC072BEE08C6");

            entity.HasIndex(e => e.ListingId, "IX_ListingImages_ListingId");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingImages)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK__ListingIm__Listi__17036CC0");
        });

        modelBuilder.Entity<MaintenanceTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Maintena__3214EC071D5966C6");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IncidentImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ResolvedImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserDescription).HasMaxLength(500);

            entity.HasOne(d => d.AssetAssignment).WithMany(p => p.MaintenanceTickets)
                .HasForeignKey(d => d.AssetAssignmentId)
                .HasConstraintName("FK__Maintenan__Asset__44CA3770");

            entity.HasOne(d => d.Resident).WithMany(p => p.MaintenanceTickets)
                .HasForeignKey(d => d.ResidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Resid__43D61337");

            entity.HasOne(d => d.Room).WithMany(p => p.MaintenanceTickets)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__RoomI__42E1EEFE");

            entity.HasOne(d => d.Technician).WithMany(p => p.MaintenanceTickets)
                .HasForeignKey(d => d.TechnicianId)
                .HasConstraintName("FK__Maintenan__Techn__45BE5BA9");

            entity.HasOne(d => d.Tenant).WithMany(p => p.MaintenanceTickets)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Tenan__41EDCAC5");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC0720D981B2");

            entity.HasIndex(e => e.PermissionKey, "UQ__Permissi__8884ABD46702546C").IsUnique();

            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PermissionKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Properti__3214EC07FE441D45");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PropertyName).HasMaxLength(150);
            entity.Property(e => e.PropertyType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Properties)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Propertie__Tenan__6A30C649");
        });

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resident__3214EC07115F9503");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IdBackImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IdFrontImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IdentityCardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Residents)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Residents__Tenan__22751F6C");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07BE72DF6E");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Roles)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Roles__TenantId__5BE2A6F2");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK__RolePermi__Permi__628FA481"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__RolePermi__RoleI__619B8048"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__RolePerm__6400A1A8CB2E4434");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rooms__3214EC077FA99C97");

            entity.HasIndex(e => new { e.PropertyId, e.Status }, "IX_Rooms_PropertyId_Status");

            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MaxOccupants).HasDefaultValue(1);
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Property).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rooms__PropertyI__72C60C4A");
        });

        modelBuilder.Entity<RoomAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoomAvai__3214EC07207FCE01");

            entity.Property(e => e.DynamicPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsBooked).HasDefaultValue(false);

            entity.HasOne(d => d.Room).WithMany(p => p.RoomAvailabilities)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomAvail__RoomI__7E37BEF6");
        });

        modelBuilder.Entity<RoomImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoomImag__3214EC079F9973C0");

            entity.HasIndex(e => e.RoomId, "IX_RoomImages_RoomId");

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsCover).HasDefaultValue(false);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomImages)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__RoomImage__RoomI__778AC167");
        });

        modelBuilder.Entity<SubscriptionPackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC077D1660D7");

            entity.HasIndex(e => e.PackageKey, "UQ__Subscrip__63953E72B6E7DAB9").IsUnique();

            entity.Property(e => e.AllowAiFeatures).HasDefaultValue(false);
            entity.Property(e => e.AllowCustomRoles).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PackageKey)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PackageName).HasMaxLength(50);
            entity.Property(e => e.PriceMonthly).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TokenGiftMonthly).HasDefaultValue(0);
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Technici__3214EC0701E593D1");

            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Specialty).HasMaxLength(50);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Technicians)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Technicia__Tenan__3D2915A8");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tenants__3214EC070623D35C");

            entity.HasIndex(e => e.PackageId, "IX_Tenants_PackageId");

            entity.HasIndex(e => e.OwnerEmail, "UQ__Tenants__FF0186BBAE6B6868").IsUnique();

            entity.Property(e => e.BusinessName).HasMaxLength(150);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OwnerEmail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OwnerPhone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubscriptionStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TaxCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TokenBalance).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Package).WithMany(p => p.Tenants)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tenants__Package__534D60F1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC078909D83A");

            entity.HasIndex(e => e.TenantId, "IX_Users_TenantId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__TenantId__5812160E");

            entity.HasMany(d => d.Properties).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPropertyMapping",
                    r => r.HasOne<Property>().WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserPrope__Prope__6E01572D"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserPrope__UserI__6D0D32F4"),
                    j =>
                    {
                        j.HasKey("UserId", "PropertyId").HasName("PK__UserProp__5084563FC247FEEC");
                        j.ToTable("UserPropertyMapping");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__UserRoles__RoleI__66603565"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserRoles__UserI__656C112C"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760AD29238DE0");
                        j.ToTable("UserRoles");
                    });
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");

            if (idProperty?.ClrType == typeof(Guid))
            {
                idProperty.SetDefaultValueSql("(newsequentialid())");
            }
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
