namespace NovaStay.Infrastructure.Models;

// The representative remains Contract.ResidentId. This table contains only co-residents.
public sealed class ContractCoResident
{
    public Guid ContractId { get; set; }
    public Guid ResidentId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public Contract Contract { get; set; } = null!;
    public Resident Resident { get; set; } = null!;
}
