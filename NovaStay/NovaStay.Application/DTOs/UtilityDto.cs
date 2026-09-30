namespace NovaStay.Application.DTOs;

// ─── DTOs ────────────────────────────────────────────────────────────────────

public sealed class UtilityMeterDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid PropertyServiceId { get; set; }
    public string? MeterCode { get; set; }
    public string MeterType { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal InitialReading { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Chỉ số ghi nhận gần nhất (null nếu chưa có reading nào)</summary>
    public UtilityReadingDto? LatestReading { get; set; }
    public string ServiceName { get; set; } = null!;
}

public sealed class UtilityReadingDto
{
    public Guid Id { get; set; }
    public Guid UtilityMeterId { get; set; }
    public DateTime ReadingDate { get; set; }
    public string BillingPeriod { get; set; } = null!;
    public decimal PreviousReading { get; set; }
    public decimal CurrentReading { get; set; }
    public decimal Consumption { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// ─── Requests ────────────────────────────────────────────────────────────────

/// <summary>Ghi chỉ số mới cho một đồng hồ</summary>
public sealed class RecordUtilityReadingRequest
{
    /// <summary>Kỳ ghi nhận (VD: "2026-09")</summary>
    public string BillingPeriod { get; set; } = null!;
    public decimal CurrentReading { get; set; }
    public string? Note { get; set; }
}

/// <summary>Tạo mới một đồng hồ dịch vụ cho phòng</summary>
public sealed class CreateUtilityMeterRequest
{
    public Guid RoomId { get; set; }
    public Guid PropertyServiceId { get; set; }
    public string? MeterCode { get; set; }
    /// <summary>"Electric" | "Water" | "Gas" | v.v.</summary>
    public string MeterType { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal InitialReading { get; set; }
}
