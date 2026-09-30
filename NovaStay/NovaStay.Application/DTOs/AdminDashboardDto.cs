namespace NovaStay.Application.DTOs;

public sealed class AdminDashboardStatsDto
{
    public int TotalOrganizations { get; set; }
    public int TotalResidents { get; set; }
    public int TotalBusinessOwnerAccounts { get; set; }
    public int TotalResidentAccounts { get; set; }
    public int TotalAllAccounts { get; set; }
}

public sealed class AdminOrganizationSummaryDto
{
    public Guid OrganizationId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessArea { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public int ResidentCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminResidentSummaryDto
{
    public Guid ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string MembershipStatus { get; set; } = string.Empty;
    public DateTime? JoinedAt { get; set; }
}
