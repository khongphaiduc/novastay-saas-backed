namespace Host.Application.DTOs;

public sealed class RoomAvailabilityDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly StayDate { get; set; }
    public decimal DynamicPrice { get; set; }
    public bool? IsBooked { get; set; }
}