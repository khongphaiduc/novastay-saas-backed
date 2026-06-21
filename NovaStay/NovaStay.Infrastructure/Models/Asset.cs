using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Asset
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string AssetName { get; set; } = null!;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? AssetCode { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public DateOnly? WarrantyExpiryDate { get; set; }

    public decimal? BaseValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

    public virtual Organization Organization { get; set; } = null!;
}
