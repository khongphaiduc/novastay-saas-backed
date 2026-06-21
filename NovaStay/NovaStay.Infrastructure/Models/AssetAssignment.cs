using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class AssetAssignment
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid AssetId { get; set; }

    public Guid? RoomId { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Asset Asset { get; set; } = null!;

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual Room? Room { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
