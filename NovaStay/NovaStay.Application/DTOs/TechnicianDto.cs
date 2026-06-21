namespace NovaStay.Application.DTOs;

public sealed class TechnicianDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool? IsAvailable { get; set; }
}