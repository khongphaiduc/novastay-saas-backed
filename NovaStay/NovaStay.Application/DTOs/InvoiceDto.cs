namespace NovaStay.Application.DTOs;

public sealed class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? BookingId { get; set; }
    public string InvoicePeriod { get; set; } = string.Empty;
    public decimal RoomPrice { get; set; }
    public decimal ServicesPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}