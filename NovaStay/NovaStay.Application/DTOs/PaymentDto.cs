namespace NovaStay.Application.DTOs;

public sealed record PaymentCapabilitiesDto(bool IsDemoAccount, bool CanUsePayOs, bool RequiresUpgrade,
    bool ConfigurationRequired, string Reason, string[] AvailableModes);
public sealed record InvoicePaymentStatusDto(Guid InvoiceId, string PaymentMode, string InvoiceStatus,
    decimal Amount, string? PaymentSource, DateTime? PaidAt, PaymentLinkDto? Payment);
public sealed record PaymentLinkDto(Guid AttemptId, long OrderCode, string Status, string? PaymentLinkId,
    string? CheckoutUrl, string? QrCode, string AccountNumber, string? AccountName, string? BankBin,
    DateTime ExpiresAt, string? ReviewReason);
public sealed record InvoicePaymentUpdated(Guid EventId, Guid InvoiceId, string Status,
    string PaymentSource, DateTime PaidAt);
