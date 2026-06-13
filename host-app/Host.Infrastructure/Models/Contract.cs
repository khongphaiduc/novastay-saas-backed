using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Contract
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }

    public Guid RoomId { get; set; }

    public Guid ResidentId { get; set; }

    public Guid? BrokerId { get; set; }

    public Guid? BookingId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal? BrokerCommission { get; set; }

    public string? CommissionStatus { get; set; }

    public string? ContractPdfUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Broker? Broker { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Property Property { get; set; } = null!;

    public virtual Resident Resident { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
