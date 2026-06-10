using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Booking : Entity
{
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? BedId { get; set; }
    public EntityName GuestName { get; set; }
    public PhoneNumber GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public string BookingType { get; set; } = string.Empty;
    public DateOnly CheckInDate { get; set; }
    public DateOnly? CheckOutDate { get; set; }
    public Money Amount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentTransactionId { get; set; }
    public DateTime? CreatedAt { get; set; }
}