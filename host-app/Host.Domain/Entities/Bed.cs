using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Bed : Entity
{
    public Guid RoomId { get; set; }
    public Code BedNumber { get; set; }
    public string? LockerId { get; set; }
    public Money BasePrice { get; set; }
    public Status Status { get; set; }
    public string? CardToken { get; set; }
}