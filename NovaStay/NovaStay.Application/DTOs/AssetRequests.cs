namespace NovaStay.Application.DTOs;

/// <summary>TASK-056: Tạo tài sản mới</summary>
public sealed class CreateAssetRequest
{
    public Guid OrganizationId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetCode { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public decimal? BaseValue { get; set; }
    /// <summary>Note tùy chọn khi tài sản mới nhập kho</summary>
    public string? InitialNote { get; set; }
}

/// <summary>TASK-057: Cập nhật thông tin tài sản</summary>
public sealed class UpdateAssetRequest
{
    public string AssetName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? AssetCode { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public decimal? BaseValue { get; set; }
}

/// <summary>TASK-058: Gán tài sản vào phòng</summary>
public sealed class AssignAssetRequest
{
    public Guid RoomId { get; set; }
    public string? Note { get; set; }
}

/// <summary>TASK-059: Thu hồi tài sản khỏi phòng</summary>
public sealed class RevokeAssetRequest
{
    public string? Note { get; set; }
}

/// <summary>TASK-060 / TASK-061: Cập nhật tình trạng tài sản / Ghi nhận hư hỏng</summary>
public sealed class UpdateAssetStatusRequest
{
    /// <summary>Các trạng thái hợp lệ: Good, Damaged, Maintenance, Broken</summary>
    public string Status { get; set; } = "Good";
    public string? Note { get; set; }
}
