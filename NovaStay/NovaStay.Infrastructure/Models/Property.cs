using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Property
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string PropertyName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string PropertyType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
}
