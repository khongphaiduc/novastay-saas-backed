namespace NovaStay.Infrastructure.Models;

// Immutable versions: issued invoices retain the bank details and QR shown when issued.
public sealed class BillingBankAccount
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
    public byte[]? QrImage { get; set; }
    public string? QrContentType { get; set; }
    public DateTime CreatedAt { get; set; }
}
