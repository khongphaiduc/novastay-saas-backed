namespace NovaStay.Application.DTOs;

public sealed class OrganizationDto
{
    public Guid Id { get; set; }
    public Guid OwnerAccountId { get; set; }
    public Guid PackageId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessArea { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public int? TokenBalance { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
