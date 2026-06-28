namespace NovaStay.Application.DTOs;

// TASK-031: Tạo hợp đồng thuê
public sealed class CreateContractRequest
{
    public Guid OrganizationId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
    public Guid ResidentId { get; set; }
    public Guid? BrokerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal? BrokerCommission { get; set; }
}

// TASK-032: Cập nhật hợp đồng
public sealed class UpdateContractRequest
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? BrokerCommission { get; set; }
    public string? CommissionStatus { get; set; }
}

// TASK-033: Gia hạn hợp đồng
public sealed class RenewContractRequest
{
    public DateOnly NewEndDate { get; set; }
    public decimal? NewDepositAmount { get; set; }
}

// TASK-034: Thanh lý hợp đồng
public sealed class TerminateContractRequest
{
    public string? Reason { get; set; }
    public string? NewRoomStatus { get; set; } // "Available" hoặc "Maintenance"
}

// TASK-037: Tìm kiếm hợp đồng (query params, dùng dưới controller)
// Dữ liệu enriched cho danh sách hợp đồng (TASK-036)
public sealed class ContractDetailDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid RoomId { get; set; }
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
    // Enriched fields
    public string? ResidentName { get; set; }
    public string? ResidentPhone { get; set; }
    public string? RoomNumber { get; set; }
    public decimal BasePrice { get; set; }
    // TASK-038: Số ngày đến khi hết hạn
    public int DaysUntilExpiry => StartDate == default ? 0 : (int)(EndDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).TotalDays;
    public bool IsExpiringSoon => DaysUntilExpiry is >= 0 and <= 30;
}
