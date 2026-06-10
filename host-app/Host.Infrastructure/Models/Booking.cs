using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Booking
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }

    public Guid RoomId { get; set; }

    public Guid? BedId { get; set; }

    public string GuestName { get; set; } = null!;

    public string GuestPhone { get; set; } = null!;

    public string? GuestEmail { get; set; }

    public string BookingType { get; set; } = null!;

    public DateOnly CheckInDate { get; set; }

    public DateOnly? CheckOutDate { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentTransactionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Bed? Bed { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Property Property { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
