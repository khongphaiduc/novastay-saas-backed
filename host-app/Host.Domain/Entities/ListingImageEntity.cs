using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class ListingImageEntity : Entity
{
    public Guid ListingId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int? DisplayOrder { get; set; }
    public DateTime? UploadedAt { get; set; }
}