using NovaStay.Domain.Enums;

namespace NovaStay.Application.DTOs;

public sealed class FinancialTransactionDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid PropertyId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public Guid? ResidentId { get; set; }
    public string? ResidentName { get; set; }
    public Guid? StaffUserId { get; set; }
    public string? StaffUserName { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string TypeRaw { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string PaymentMethodRaw { get; set; } = string.Empty;
    public ApprovalStatus? Status { get; set; }
    public string StatusRaw { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
