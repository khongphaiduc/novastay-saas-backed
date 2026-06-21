namespace NovaStay.Application.DTOs;

public sealed class RoomDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? MaxOccupants { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime? CreatedAt { get; set; }
}