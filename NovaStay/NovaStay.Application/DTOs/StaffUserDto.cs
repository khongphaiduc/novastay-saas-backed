namespace NovaStay.Application.DTOs;

public sealed class StaffUserDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid OrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
