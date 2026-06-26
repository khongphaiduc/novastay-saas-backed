using System;

namespace NovaStay.Infrastructure.Models;

public partial class RoomService
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Guid PropertyServiceId { get; set; }

    public decimal? PriceOverride { get; set; }

    public bool IsActive { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? RemovedAt { get; set; }

    public string? Note { get; set; }

    public virtual PropertyService PropertyService { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
