using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

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

    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
