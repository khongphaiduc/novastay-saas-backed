using System;

namespace NovaStay.Infrastructure.Models;

public partial class InvoiceGenerationSchedule
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? PropertyId { get; set; }

    public string ScheduleName { get; set; } = null!;

    public string BillingCycle { get; set; } = null!;

    public int GenerateDayOfMonth { get; set; }

    public TimeOnly GenerateTime { get; set; }

    public string TimeZone { get; set; } = null!;

    public int DueAfterDays { get; set; }

    public bool AutoSendToResident { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastRunAt { get; set; }

    public DateTime? NextRunAt { get; set; }

    public Guid? CreatedByStaffUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual StaffUser? CreatedByStaffUser { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Property? Property { get; set; }
}
