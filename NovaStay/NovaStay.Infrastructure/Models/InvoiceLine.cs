using System;

namespace NovaStay.Infrastructure.Models;

public partial class InvoiceLine
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public string LineType { get; set; } = null!;

    public string? SourceType { get; set; }

    public Guid? SourceId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineAmount { get; set; }

    public DateOnly? BillingStartDate { get; set; }

    public DateOnly? BillingEndDate { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDebit { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
