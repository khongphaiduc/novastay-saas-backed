using System;
using System.Collections.Generic;

namespace Host.Infrastructure.Models;

public partial class User
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<UserAccessToken> UserAccessTokens { get; set; } = new List<UserAccessToken>();

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();
}
