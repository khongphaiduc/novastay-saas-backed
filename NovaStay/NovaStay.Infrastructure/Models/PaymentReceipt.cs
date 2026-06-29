using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class PaymentReceipt
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? PropertyId { get; set; }

    public Guid? ResidentId { get; set; }

    public Guid? CollectedByStaffUserId { get; set; }

    public string ReceiptNumber { get; set; } = null!;

    public string ReceiptType { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaidAt { get; set; }

    public string? ReferenceCode { get; set; }

    public string Status { get; set; } = null!;

    public string? PayerName { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual StaffUser? CollectedByStaffUser { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual Property? Property { get; set; }

    public virtual Resident? Resident { get; set; }
}
