namespace NovaStay.Application.DTOs;

public sealed class RoomImageDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool? IsCover { get; set; }
    public DateTime? UploadedAt { get; set; }
}