using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class Technician
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Specialty { get; set; } = null!;

    public bool? IsAvailable { get; set; }

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual Tenant Tenant { get; set; } = null!;
}
