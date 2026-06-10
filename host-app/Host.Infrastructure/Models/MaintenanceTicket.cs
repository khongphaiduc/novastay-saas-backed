using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class MaintenanceTicket
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RoomId { get; set; }

    public Guid ResidentId { get; set; }

    public Guid? AssetAssignmentId { get; set; }

    public string Category { get; set; } = null!;

    public string UserDescription { get; set; } = null!;

    public string? IncidentImageUrl { get; set; }

    public string? Status { get; set; }

    public Guid? TechnicianId { get; set; }

    public string? ResolvedImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AssetAssignment? AssetAssignment { get; set; }

    public virtual Resident Resident { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual Technician? Technician { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
