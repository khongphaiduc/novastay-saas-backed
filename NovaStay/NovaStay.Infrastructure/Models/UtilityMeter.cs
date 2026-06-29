using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class UtilityMeter
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Guid PropertyServiceId { get; set; }

    public string? MeterCode { get; set; }

    public string MeterType { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal InitialReading { get; set; }

    public DateTime? InstalledAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual PropertyService PropertyService { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<UtilityReading> UtilityReadings { get; set; } = new List<UtilityReading>();
}
