namespace Host.Application.DTOs;

public sealed class BookingDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? BedId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string BookingType { get; set; } = string.Empty;
    public DateOnly CheckInDate { get; set; }
    public DateOnly? CheckOutDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentTransactionId { get; set; }
    public DateTime? CreatedAt { get; set; }
}