namespace Host.Application.DTOs;

public sealed class AssetDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetCode { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public decimal? BaseValue { get; set; }
    public DateTime? CreatedAt { get; set; }
}