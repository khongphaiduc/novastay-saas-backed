using System;

namespace NovaStay.Infrastructure.Models;

public partial class PaymentAllocation
{
    public Guid Id { get; set; }

    public Guid PaymentReceiptId { get; set; }

    public Guid InvoiceId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual PaymentReceipt PaymentReceipt { get; set; } = null!;
}
