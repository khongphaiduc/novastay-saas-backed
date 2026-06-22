using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Resident
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? IdentityCardNumber { get; set; }

    public string? IdFrontImageUrl { get; set; }

    public string? IdBackImageUrl { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual ICollection<ResidentMembership> ResidentMemberships { get; set; } = new List<ResidentMembership>();
}
