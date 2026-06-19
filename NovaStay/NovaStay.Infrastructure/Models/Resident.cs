using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Resident
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? IdentityCardNumber { get; set; }

    public string? IdFrontImageUrl { get; set; }

    public string? IdBackImageUrl { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual Tenant Tenant { get; set; } = null!;
}
