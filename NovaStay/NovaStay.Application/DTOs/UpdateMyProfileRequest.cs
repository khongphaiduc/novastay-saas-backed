namespace NovaStay.Application.DTOs;

/// <summary>
/// TASK-053: Cư dân tự cập nhật hồ sơ cá nhân của mình (PUT /api/residents/me/profile)
/// </summary>
public sealed class UpdateMyProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
