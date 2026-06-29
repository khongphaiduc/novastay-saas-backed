using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Invoice
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? PropertyId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? ResidentId { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? BookingId { get; set; }

    public string? InvoiceNumber { get; set; }

    public string InvoicePeriod { get; set; } = null!;

    public string? InvoiceType { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? IssuedAt { get; set; }

    public decimal RoomPrice { get; set; }

    public decimal ServicesPrice { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal AdjustmentAmount { get; set; }

    public decimal LateFeeAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? QrCodeUrl { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual PaymentReceipt? PaymentReceipt { get; set; }

    public virtual Property? Property { get; set; }

    public virtual Resident? Resident { get; set; }

    public virtual Room? Room { get; set; }
}
