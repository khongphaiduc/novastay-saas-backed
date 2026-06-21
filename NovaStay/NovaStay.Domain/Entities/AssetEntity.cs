using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class AssetEntity : Entity
{
    public Guid OrganizationId { get; set; }
    public EntityName AssetName { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetCode { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public decimal? BaseValue { get; set; }
    public DateTime? CreatedAt { get; set; }
}
