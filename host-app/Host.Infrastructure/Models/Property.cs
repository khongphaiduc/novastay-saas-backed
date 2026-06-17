using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Property
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string PropertyName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string PropertyType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
