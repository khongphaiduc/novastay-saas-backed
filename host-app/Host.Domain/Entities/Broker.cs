using Host.Domain.ValueObject;

namespace Host.Domain.Entities;

public sealed class Broker : Entity
{
    public Guid TenantId { get; set; }
    public EntityName FullName { get; set; }
    public PhoneNumber Phone { get; set; }
    public decimal? WalletBalance { get; set; }
    public decimal? TotalCommissionEarned { get; set; }
}