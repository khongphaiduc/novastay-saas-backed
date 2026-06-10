using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class RoomAvailability
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public DateOnly StayDate { get; set; }

    public decimal DynamicPrice { get; set; }

    public bool? IsBooked { get; set; }

    public virtual Room Room { get; set; } = null!;
}
