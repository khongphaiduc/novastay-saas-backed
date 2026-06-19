using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Role
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RoleName { get; set; } = null!;

    public string RoleKey { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
