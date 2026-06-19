using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Invoice
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? BookingId { get; set; }

    public string InvoicePeriod { get; set; } = null!;

    public decimal RoomPrice { get; set; }

    public decimal ServicesPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public string? QrCodeUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
