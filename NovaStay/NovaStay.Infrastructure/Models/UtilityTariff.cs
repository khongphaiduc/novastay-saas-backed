using System;

namespace NovaStay.Infrastructure.Models;

public partial class UtilityTariff
{
    public Guid Id { get; set; }

    public Guid PropertyServiceId { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public decimal UnitPrice { get; set; }

    public string PricingMode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual PropertyService PropertyService { get; set; } = null!;
}
