using NovaStay.Domain.Enums;

namespace NovaStay.Application.DTOs;

public sealed class CreateIncomeReceiptRequest
{
    public Guid? IncomeCategoryId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? ResidentId { get; set; }
    public Guid? CollectedByStaffUserId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public IncomeCategory IncomeType { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CollectedAt { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
}
