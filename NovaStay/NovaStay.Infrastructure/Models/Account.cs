using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Account
{
    public Guid Id { get; set; }

    public string AccountType { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string? Email { get; set; }

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AccountRefreshToken> AccountRefreshTokens { get; set; } = new List<AccountRefreshToken>();

    public virtual ICollection<Organization> OwnedOrganizations { get; set; } = new List<Organization>();

    public virtual ICollection<Resident> Residents { get; set; } = new List<Resident>();

    public virtual ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
}
