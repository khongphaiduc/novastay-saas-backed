namespace NovaStay.Application.DTOs;

public sealed class BrokerDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal? WalletBalance { get; set; }
    public decimal? TotalCommissionEarned { get; set; }
}