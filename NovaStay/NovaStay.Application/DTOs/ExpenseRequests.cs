using NovaStay.Domain.Enums;

namespace NovaStay.Application.DTOs;

public sealed class CreateExpenseRequest
{
    public Guid? ExpenseCategoryId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? RelatedMaintenanceTicketId { get; set; }
    public Guid? RelatedBrokerId { get; set; }
    public Guid? ApprovedByStaffUserId { get; set; }
    public Guid? CreatedByStaffUserId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public ExpenseCategory ExpenseType { get; set; }
    public string PayeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime SpentAt { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
}
