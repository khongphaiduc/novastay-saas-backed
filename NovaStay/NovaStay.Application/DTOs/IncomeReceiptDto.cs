using NovaStay.Domain.Enums;

namespace NovaStay.Application.DTOs;

public sealed class IncomeReceiptDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid IncomeCategoryId { get; set; }
    public string IncomeCategoryCode { get; set; } = string.Empty;
    public string IncomeCategoryName { get; set; } = string.Empty;
    public Guid? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public Guid? ResidentId { get; set; }
    public string? ResidentName { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public IncomeCategory? IncomeType { get; set; }
    public string IncomeTypeRaw { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CollectedAt { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string PaymentMethodRaw { get; set; } = string.Empty;
    public ApprovalStatus? Status { get; set; }
    public string StatusRaw { get; set; } = string.Empty;
    public Guid? CollectedByStaffUserId { get; set; }
    public string? CollectedByStaffUserName { get; set; }
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
