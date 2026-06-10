namespace Host.Application.DTOs;

public sealed class BedDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string BedNumber { get; set; } = string.Empty;
    public string? LockerId { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CardToken { get; set; }
}