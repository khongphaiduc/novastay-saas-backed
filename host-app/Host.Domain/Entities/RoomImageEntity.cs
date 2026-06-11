using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class RoomImageEntity : Entity
{
    public Guid RoomId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool? IsCover { get; set; }
    public DateTime? UploadedAt { get; set; }
}