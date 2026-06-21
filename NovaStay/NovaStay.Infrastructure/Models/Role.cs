using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Role
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string RoleName { get; set; } = null!;

    public string RoleKey { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
}
