namespace NovaStay.Infrastructure.Models;

public sealed class PayOsPaymentAttempt
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public long OrderCode { get; set; }
    public Guid ChannelOwnerAccountId { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = "Creating";
    public string? PaymentLinkId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrCode { get; set; }
    public string AccountNumber { get; set; } = "";
    public string? AccountName { get; set; }
    public string? BankBin { get; set; }
    public string? ProviderReference { get; set; }
    public string? ReviewReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[]? Version { get; set; }
}

public sealed class PaymentNotificationOutbox
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid OwnerAccountId { get; set; }
    public Guid? ResidentAccountId { get; set; }
    public string PaymentSource { get; set; } = "";
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public int Attempts { get; set; }
}
