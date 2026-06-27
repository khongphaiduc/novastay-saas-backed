using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class Organization
{
    public Guid Id { get; set; }

    public Guid OwnerAccountId { get; set; }

    public Guid PackageId { get; set; }

    public string BusinessName { get; set; } = null!;

    public string BusinessArea { get; set; } = null!;

    public string? TaxCode { get; set; }

    public string OwnerEmail { get; set; } = null!;

    public string OwnerPhone { get; set; } = null!;

    public string SubscriptionStatus { get; set; } = null!;

    public int? TokenBalance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Broker> Brokers { get; set; } = new List<Broker>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<ExpenseCategory> ExpenseCategories { get; set; } = new List<ExpenseCategory>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    public virtual Account OwnerAccount { get; set; } = null!;

    public virtual SubscriptionPackage Package { get; set; } = null!;

    public virtual ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<ResidentMembership> ResidentMemberships { get; set; } = new List<ResidentMembership>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<Technician> Technicians { get; set; } = new List<Technician>();

    public virtual ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
}
