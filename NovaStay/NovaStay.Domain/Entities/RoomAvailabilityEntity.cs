using NovaStay.Domain.ValueObject;

namespace NovaStay.Domain.Entities;

public sealed class RoomAvailabilityEntity : Entity
{
    public Guid RoomId { get; set; }
    public DateOnly StayDate { get; set; }
    public Money DynamicPrice { get; set; }
    public bool? IsBooked { get; set; }
}