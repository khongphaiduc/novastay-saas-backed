using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class RoomEntity : Entity
{
    public Guid PropertyId { get; set; }
    public Code RoomNumber { get; set; }
    public int Floor { get; set; }
    public Money BasePrice { get; set; }
    public Status Status { get; set; }
    public int? MaxOccupants { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime? CreatedAt { get; set; }
}