namespace Host.Application.DTOs;

public sealed class ContractDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? BedId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid? BrokerId { get; set; }
    public Guid? BookingId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal? BrokerCommission { get; set; }
    public string? CommissionStatus { get; set; }
    public string? ContractPdfUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}