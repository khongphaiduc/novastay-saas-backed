using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class StaffUser
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public Guid OrganizationId { get; set; }

    public string FullName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

}
