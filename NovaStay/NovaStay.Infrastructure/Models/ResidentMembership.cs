using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class ResidentMembership
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public Guid ResidentId { get; set; }

    public Guid OrganizationId { get; set; }

    public string MembershipCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? InvitedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public DateTime? JoinedAt { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Resident Resident { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
