using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Bed
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string BedNumber { get; set; } = null!;

    public string? LockerId { get; set; }

    public decimal BasePrice { get; set; }

    public string Status { get; set; } = null!;

    public string? CardToken { get; set; }

    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual Room Room { get; set; } = null!;
}
