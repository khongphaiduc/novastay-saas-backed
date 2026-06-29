using System;

namespace NovaStay.Infrastructure.Models;

public partial class UtilityReading
{
    public Guid Id { get; set; }

    public Guid UtilityMeterId { get; set; }

    public DateTime ReadingDate { get; set; }

    public string BillingPeriod { get; set; } = null!;

    public decimal PreviousReading { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal Consumption { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public Guid? RecordedByStaffUserId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual StaffUser? RecordedByStaffUser { get; set; }

    public virtual UtilityMeter UtilityMeter { get; set; } = null!;
}
