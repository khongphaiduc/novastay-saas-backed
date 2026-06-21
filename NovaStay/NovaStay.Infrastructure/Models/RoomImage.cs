using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class RoomImage
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsCover { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual Room Room { get; set; } = null!;
}
