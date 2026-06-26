using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class PropertyService
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? ServiceCode { get; set; }

    public string? Description { get; set; }

    public decimal DefaultPrice { get; set; }

    public string? Unit { get; set; }

    public string? BillingCycle { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Property Property { get; set; } = null!;

    public virtual ICollection<RoomService> RoomServices { get; set; } = new List<RoomService>();
}
