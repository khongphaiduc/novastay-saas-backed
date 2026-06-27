using System;

namespace NovaStay.Infrastructure.Models;

public partial class Expense
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ExpenseCategoryId { get; set; }

    public Guid? PropertyId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? RelatedMaintenanceTicketId { get; set; }

    public Guid? RelatedBrokerId { get; set; }

    public Guid? ApprovedByStaffUserId { get; set; }

    public Guid? CreatedByStaffUserId { get; set; }

    public string ExpenseNumber { get; set; } = null!;

    public string ExpenseType { get; set; } = null!;

    public string PayeeName { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime SpentAt { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ReferenceCode { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual StaffUser? ApprovedByStaffUser { get; set; }

    public virtual StaffUser? CreatedByStaffUser { get; set; }

    public virtual ExpenseCategory ExpenseCategory { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual Property? Property { get; set; }

    public virtual Broker? RelatedBroker { get; set; }

    public virtual MaintenanceTicket? RelatedMaintenanceTicket { get; set; }

    public virtual Room? Room { get; set; }
}
