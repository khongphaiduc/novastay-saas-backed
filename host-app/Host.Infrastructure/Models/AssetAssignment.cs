using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class AssetAssignment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? BedId { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Asset Asset { get; set; } = null!;

    public virtual Bed? Bed { get; set; }

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual Room? Room { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
