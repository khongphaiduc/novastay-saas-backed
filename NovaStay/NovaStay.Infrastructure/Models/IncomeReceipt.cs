using System;

namespace NovaStay.Infrastructure.Models;

public partial class IncomeReceipt
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid IncomeCategoryId { get; set; }

    public Guid PropertyId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? ResidentId { get; set; }

    public Guid? CollectedByStaffUserId { get; set; }

    public string ReceiptNumber { get; set; } = null!;

    public string IncomeType { get; set; } = null!;

    public string PayerName { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CollectedAt { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ReferenceCode { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual StaffUser? CollectedByStaffUser { get; set; }

    public virtual IncomeCategory IncomeCategory { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;

    public virtual Resident? Resident { get; set; }

    public virtual Room? Room { get; set; }
}
