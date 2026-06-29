using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Property
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string PropertyName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string PropertyType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<IncomeReceipt> IncomeReceipts { get; set; } = new List<IncomeReceipt>();

    public virtual ICollection<InvoiceGenerationSchedule> InvoiceGenerationSchedules { get; set; } = new List<InvoiceGenerationSchedule>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();

    public virtual ICollection<PropertyService> PropertyServices { get; set; } = new List<PropertyService>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
}
