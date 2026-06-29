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

    public virtual ICollection<Expense> ApprovedExpenses { get; set; } = new List<Expense>();

    public virtual ICollection<Expense> CreatedExpenses { get; set; } = new List<Expense>();

    public virtual ICollection<IncomeReceipt> CollectedIncomeReceipts { get; set; } = new List<IncomeReceipt>();

    public virtual ICollection<InvoiceGenerationSchedule> InvoiceGenerationSchedules { get; set; } = new List<InvoiceGenerationSchedule>();

    public virtual ICollection<PaymentReceipt> CollectedPaymentReceipts { get; set; } = new List<PaymentReceipt>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<UtilityReading> UtilityReadings { get; set; } = new List<UtilityReading>();

}
