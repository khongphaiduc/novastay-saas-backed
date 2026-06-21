using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Broker
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public decimal? WalletBalance { get; set; }

    public decimal? TotalCommissionEarned { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual Organization Organization { get; set; } = null!;
}
