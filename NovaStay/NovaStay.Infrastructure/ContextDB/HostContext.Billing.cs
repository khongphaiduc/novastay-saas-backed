using Microsoft.EntityFrameworkCore;
using NovaStay.Infrastructure.Models;

namespace NovaStay.Infrastructure.ContextDB;

public partial class HostContext
{
    public DbSet<BillingBankAccount> BillingBankAccounts { get; set; }
    public DbSet<PayOsPaymentAttempt> PayOsPaymentAttempts { get; set; }
    public DbSet<PaymentNotificationOutbox> PaymentNotificationOutbox { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        ConfigureCoResidents(modelBuilder);
        modelBuilder.Entity<BillingBankAccount>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.BankName).HasMaxLength(100);
            e.Property(x => x.AccountName).HasMaxLength(150);
            e.Property(x => x.AccountNumber).HasMaxLength(50);
            e.Property(x => x.QrContentType).HasMaxLength(30);
            e.HasIndex(x => new { x.OrganizationId, x.CreatedAt });
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Invoice>(e =>
        {
            e.Property(x => x.PaymentMode).HasMaxLength(20).HasDefaultValue("Manual");
            e.Property(x => x.PaymentSource).HasMaxLength(20);
            e.Property(x => x.PaymentReference).HasMaxLength(100);
            e.Property(x => x.ConfirmationNote).HasMaxLength(500);
            e.Property(x => x.BillingVersion).IsRowVersion();
            e.HasOne(x => x.BillingBankAccount).WithMany().HasForeignKey(x => x.BillingBankAccountId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PayOsPaymentAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.OrderCode).IsUnique();
            e.HasIndex(x => x.InvoiceId).IsUnique().HasFilter("[IsActive] = 1").HasDatabaseName("UX_PayOsPaymentAttempts_ActiveInvoice");
            e.HasIndex(x => new { x.ChannelOwnerAccountId, x.ProviderReference }).IsUnique().HasFilter("[ProviderReference] IS NOT NULL");
            e.HasIndex(x => new { x.Status, x.UpdatedAt });
            e.Property(x => x.Amount).HasColumnType("decimal(18,0)");
            e.Property(x => x.Status).HasMaxLength(30);
            e.Property(x => x.PaymentLinkId).HasMaxLength(100);
            e.Property(x => x.ProviderReference).HasMaxLength(100);
            e.Property(x => x.CheckoutUrl).HasMaxLength(1024);
            e.Property(x => x.QrCode).HasMaxLength(4096);
            e.Property(x => x.AccountNumber).HasMaxLength(100);
            e.Property(x => x.AccountName).HasMaxLength(150);
            e.Property(x => x.BankBin).HasMaxLength(20);
            e.Property(x => x.ReviewReason).HasMaxLength(100);
            e.Property(x => x.Version).IsRowVersion();
        });
        modelBuilder.Entity<PaymentNotificationOutbox>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PaymentSource).HasMaxLength(20);
            e.HasIndex(x => x.InvoiceId).IsUnique();
            e.HasIndex(x => new { x.SentAt, x.NextAttemptAt });
            e.HasOne<Invoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<InvoiceLine>(e =>
        {
            e.Property(x => x.Quantity).HasColumnType("decimal(18, 3)");
            e.Property(x => x.BillingPeriod).HasMaxLength(7).IsUnicode(false);
            e.HasIndex(x => new { x.BillingOrganizationId, x.BillingRoomId, x.BillingPeriod, x.SourceType, x.SourceId })
                .IsUnique().HasFilter("[BillingPeriod] IS NOT NULL AND [SourceId] IS NOT NULL AND [SourceType] IS NOT NULL AND [BillingOrganizationId] IS NOT NULL AND [BillingRoomId] IS NOT NULL")
                .HasDatabaseName("UX_InvoiceLines_BillingCharge");
        });
    }
}
