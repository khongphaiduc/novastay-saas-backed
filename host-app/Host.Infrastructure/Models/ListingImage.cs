using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class ListingImage
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
