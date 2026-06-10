using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Listing
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }

    public Guid RoomId { get; set; }

    public Guid? BedId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Amenities { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Bed? Bed { get; set; }

    public virtual ICollection<ListingImage> ListingImages { get; set; } = new List<ListingImage>();

    public virtual Property Property { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
