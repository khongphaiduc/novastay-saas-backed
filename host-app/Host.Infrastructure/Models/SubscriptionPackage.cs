using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class SubscriptionPackage
{
    public Guid Id { get; set; }

    public string PackageName { get; set; } = null!;

    public string PackageKey { get; set; } = null!;

    public decimal PriceMonthly { get; set; }

    public int MaxProperties { get; set; }

    public int MaxRooms { get; set; }

    public int? TokenGiftMonthly { get; set; }

    public bool? AllowCustomRoles { get; set; }

    public bool? AllowAiFeatures { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
