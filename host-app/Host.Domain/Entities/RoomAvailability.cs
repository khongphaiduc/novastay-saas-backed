using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class RoomAvailability : Entity
{
    public Guid RoomId { get; set; }
    public DateOnly StayDate { get; set; }
    public Money DynamicPrice { get; set; }
    public bool? IsBooked { get; set; }
}