using System;
using System.Collections.Generic;
using NovaStay.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace NovaStay.Infrastructure.ContextDB;

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

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Broker> Brokers { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<Resident> Residents { get; set; }

    public virtual DbSet<ResidentMembership> ResidentMemberships { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomAvailability> RoomAvailabilities { get; set; }

    public virtual DbSet<RoomImage> RoomImages { get; set; }

    public virtual DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }

    public virtual DbSet<Technician> Technicians { get; set; }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<StaffUser> StaffUsers { get; set; }

    public virtual DbSet<AccountRefreshToken> AccountRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Accounts");

            entity.HasIndex(e => e.Email, "IX_Accounts_Email");
            entity.HasIndex(e => e.Phone, "IX_Accounts_Phone").IsUnique();

            entity.Property(e => e.AccountType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.MustSetPassword).HasDefaultValue(false);
            entity.Property(e => e.PasswordSetAt).HasColumnType("datetime");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

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

            entity.HasOne(d => d.Organization).WithMany(p => p.Assets)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assets__OrganizationId__02FC7413");
        });

        modelBuilder.Entity<AssetAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetAss__3214EC073B842B58");

            entity.HasIndex(e => e.RoomId, "IX_AssetAssignments_RoomId");

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

            entity.HasOne(d => d.Room).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__AssetAssi__RoomI__09A971A2");

            entity.HasOne(d => d.Organization).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AssetAssi__Tenan__07C12930");
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

            entity.HasOne(d => d.Property).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__Proper__1CBC4616");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__RoomId__1DB06A4F");

            entity.HasOne(d => d.Organization).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__Organization__1BC821DD");
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

            entity.HasOne(d => d.Organization).WithMany(p => p.Brokers)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Brokers__OrganizationI__2739D489");
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

            entity.HasOne(d => d.Organization).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.OrganizationId)
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

            entity.HasOne(d => d.Organization).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Organization__37703C52");
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

            entity.HasOne(d => d.Organization).WithMany(p => p.MaintenanceTickets)
                .HasForeignKey(d => d.OrganizationId)
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

            entity.HasOne(d => d.Organization).WithMany(p => p.Properties)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Propertie__Tenan__6A30C649");
        });

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resident__3214EC07115F9503");

            entity.HasIndex(e => e.AccountId, "IX_Residents_AccountId").IsUnique();

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

            entity.HasOne(d => d.Account).WithMany(p => p.Residents)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Residents_Accounts_AccountId");
        });

        modelBuilder.Entity<ResidentMembership>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ResidentMemberships");

            entity.HasIndex(e => e.AccountId, "IX_ResidentMemberships_AccountId");

            entity.HasIndex(e => e.OrganizationId, "IX_ResidentMemberships_OrganizationId");

            entity.HasIndex(e => e.ResidentId, "IX_ResidentMemberships_ResidentId");

            entity.HasIndex(e => e.MembershipCode, "UX_ResidentMemberships_MembershipCode")
                .IsUnique();

            entity.HasIndex(e => new { e.AccountId, e.OrganizationId }, "UX_ResidentMemberships_AccountId_OrganizationId")
                .IsUnique();

            entity.Property(e => e.ActivatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InvitedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RespondedAt).HasColumnType("datetime");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MembershipCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Account).WithMany(p => p.ResidentMemberships)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResidentMemberships_Accounts_AccountId");

            entity.HasOne(d => d.Organization).WithMany(p => p.ResidentMemberships)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResidentMemberships_Organizations_OrganizationId");

            entity.HasOne(d => d.Resident).WithMany(p => p.ResidentMemberships)
                .HasForeignKey(d => d.ResidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResidentMemberships_Residents_ResidentId");
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

            entity.HasOne(d => d.Organization).WithMany(p => p.Roles)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Roles__OrganizationId__5BE2A6F2");

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

            entity.HasOne(d => d.Organization).WithMany(p => p.Technicians)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Technicia__Tenan__3D2915A8");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Organizations__3214EC070623D35C");

            entity.HasIndex(e => e.OwnerAccountId, "IX_Organizations_OwnerAccountId");

            entity.HasIndex(e => e.PackageId, "IX_Organizations_PackageId");

            entity.HasIndex(e => e.OwnerEmail, "UQ__Organizations__FF0186BBAE6B6868").IsUnique();

            entity.Property(e => e.BusinessName).HasMaxLength(150);
            entity.Property(e => e.BusinessArea).HasMaxLength(100);
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

            entity.HasOne(d => d.OwnerAccount).WithMany(p => p.OwnedOrganizations)
                .HasForeignKey(d => d.OwnerAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Organizations_Accounts_OwnerAccountId");

            entity.HasOne(d => d.Package).WithMany(p => p.Organizations)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Organizations__Package__534D60F1");
        });

        modelBuilder.Entity<StaffUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC078909D83A");

            entity.HasIndex(e => e.AccountId, "IX_StaffUsers_AccountId").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "IX_Users_OrganizationId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Account).WithMany(p => p.StaffUsers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StaffUsers_Accounts_AccountId");

            entity.HasOne(d => d.Organization).WithMany(p => p.StaffUsers)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__OrganizationId__5812160E");

            entity.HasMany(d => d.Properties).WithMany(p => p.StaffUsers)
                .UsingEntity<Dictionary<string, object>>(
                    "StaffUserPropertyMapping",
                    r => r.HasOne<Property>().WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserPrope__Prope__6E01572D"),
                    l => l.HasOne<StaffUser>().WithMany()
                        .HasForeignKey("StaffUserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserPrope__UserI__6D0D32F4"),
                    j =>
                    {
                        j.HasKey("StaffUserId", "PropertyId").HasName("PK__UserProp__5084563FC247FEEC");
                        j.ToTable("StaffUserPropertyMapping");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.StaffUsers)
                .UsingEntity<Dictionary<string, object>>(
                    "StaffUserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__StaffUserRoles__RoleI__66603565"),
                    l => l.HasOne<StaffUser>().WithMany()
                        .HasForeignKey("StaffUserId")
                        .HasConstraintName("FK__StaffUserRoles__UserI__656C112C"),
                    j =>
                    {
                        j.HasKey("StaffUserId", "RoleId").HasName("PK__UserRole__AF2760AD29238DE0");
                        j.ToTable("StaffUserRoles");
                    });
        });

        modelBuilder.Entity<AccountRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AccountRefreshTokens");

            entity.HasIndex(e => e.TokenHash, "IX_AccountRefreshTokens_TokenHash")
                .IsUnique();

            entity.HasIndex(e => new { e.AccountId, e.ExpiresAt }, "IX_AccountRefreshTokens_AccountId_ExpiresAt");

            entity.Property(e => e.TokenHash)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.ReplacedByTokenHash)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.RevokedByIp)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRefreshTokens)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_AccountRefreshTokens_Accounts_AccountId");
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
