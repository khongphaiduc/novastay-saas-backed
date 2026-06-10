namespace Host.Application.DTOs;

public sealed class ListingImageDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int? DisplayOrder { get; set; }
    public DateTime? UploadedAt { get; set; }
}