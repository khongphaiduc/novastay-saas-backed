using NovaStay.Domain.Enums;

namespace NovaStay.Application.DTOs;

public sealed class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryCode { get; set; } = string.Empty;
    public string ExpenseCategoryName { get; set; } = string.Empty;
    public Guid? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public ExpenseCategory? ExpenseType { get; set; }
    public string ExpenseTypeRaw { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime SpentAt { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string PaymentMethodRaw { get; set; } = string.Empty;
    public ApprovalStatus? Status { get; set; }
    public string StatusRaw { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
    public Guid? ApprovedByStaffUserId { get; set; }
    public string? ApprovedByStaffUserName { get; set; }
    public Guid? CreatedByStaffUserId { get; set; }
    public string? CreatedByStaffUserName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
