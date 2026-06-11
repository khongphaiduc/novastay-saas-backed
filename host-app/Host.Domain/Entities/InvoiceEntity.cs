using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class InvoiceEntity : Entity
{
    public Guid TenantId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? BookingId { get; set; }
    public Code InvoicePeriod { get; set; }
    public Money RoomPrice { get; set; }
    public Money ServicesPrice { get; set; }
    public Money TotalAmount { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}