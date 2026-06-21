using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Permission
{
    public Guid Id { get; set; }

    public string PermissionName { get; set; } = null!;

    public string PermissionKey { get; set; } = null!;

    public string Module { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
