using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Room
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public int Floor { get; set; }

    public decimal BasePrice { get; set; }

    public string Status { get; set; } = null!;

    public int? MaxOccupants { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual Property Property { get; set; } = null!;

    public virtual ICollection<RoomAvailability> RoomAvailabilities { get; set; } = new List<RoomAvailability>();

    public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
}
